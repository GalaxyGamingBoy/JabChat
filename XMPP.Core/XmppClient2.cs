using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Features;

namespace XMPP.Core;

public class XmppClient2 : IXmppClient
{
  #region Properties

  /// <summary>
  /// The XMPP Address that the client will connect to
  /// </summary>
  public required XmppAddress Address { get; init; }
  
  /// <summary>
  /// The XMPP Credentials for the client to use
  /// </summary>
  public required XmppCreds Credentials { get; init; }
  
  /// <summary>
  /// If a server has optional TLS support, prefer to use it instead of plain
  /// </summary>
  public bool PreferTlsWhenOptional { get; init; } = true;

  private event EventHandler OnFeaturesReceivedAsync;
  private event EventHandler OnMessageReceivedAsync;
  
  private TcpClient? Client { get; set; } = null;
  private NetworkStream? Stream { get; set; } = null;
  private bool? IsConnected => Client?.Connected;
  private bool IsXmppStreamOpen { get; set; } = false;
  private Dictionary<string, XmlSerializer> FeatureSerializers { get; } = new();
  private Task? BackgroundServiceTask { get; set; }

  #endregion

  #region Initialization
  
  public XmppClient2()
  {
    RegisterFeature<StartTlsFeature>();
    RegisterFeature<SaslFeature>();
    RegisterFeature<BindFeature>();

    BackgroundServiceTask = Task.Run(BackgroundService);
  }
  
  #endregion

  #region Validation Guards

  private Result ValidateTcpConnection()
  {
    return Result.Merge(
      Result.FailIf(Client is null, "No client instantiated"),
      Result.FailIf(Stream is null, "No stream instantiated")
    );
  }

  private Result ValidateXmppConnection()
  {
    return Result.Merge(
      ValidateTcpConnection(),
      Result.FailIf(!IsXmppStreamOpen, "XMPP stream is not open")
    );
  }

  /// <summary>
  /// Performs TCP connection validation
  /// </summary>
  /// <returns>Validation result</returns>
  private Result ValidateConnection()
  {
    if (Client is null)
      return Result.Fail("No client connected");
    if ((bool)(!IsConnected)!)
      return Result.Fail("Cannot start a stream on a non-connected client");
    if (Stream is null)
      return Result.Fail("Cannot start a null stream");

    return Result.Ok();
  }

  #endregion
  
  #region TCP Client
  


  /// <summary>
  /// Creates a new TCP client for use by the XmppClient
  /// </summary>
  /// <remarks>If a client already exists, function will return early to prevent losing the current context</remarks>
  /// <returns>Creation result</returns>
  [Obsolete("This method is deprecated, please use NewTcpClient instead.")]
  private Result CreateTcpClient()
  {
    if (!ValidateTcpConnection().IsFailed)
      return Result.Fail("TCP client already is connected");

    Client = new TcpClient();
    Stream = null;
    
    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    return Result.Ok();
  }

  private TcpClient NewTcpClient()
  {
    var client = new TcpClient();
    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    return client;
  }

  /// <summary>
  /// Disposes the current TCP Client
  /// </summary>
  /// <remarks>Function will return early if no TCP Client exists or XMPP stream open. Does not handle XMPP stream</remarks>
  /// <returns>Disposal result</returns>
  private async Task DisposeTcpConnectionAsync()
  {
    if (Stream is not null)
    {
      await Stream.DisposeAsync();
      Stream = null;
    }

    if (Client is not null)
    {
      Client.Dispose();
      Client = null;
    }
  }

  private async Task DisposeXmppConnectionAsync()
  {
    if (IsXmppStreamOpen is true)
      await CloseStream();
  }

  private async Task DisposeConnectionAsync()
  {
    await DisposeXmppConnectionAsync();
    await DisposeTcpConnectionAsync();
  }
  
  public Task ConnectAsync(XmppAddress address)
  {
    throw new NotImplementedException();
  }

  public async Task<Result> ConnectAsync()
  {
    if (ValidateTcpConnection().IsSuccess)
      return Result.Fail("A TCP client is already connected");
    
    try
    {
      Client = NewTcpClient();
      await Client!.ConnectAsync(Address.Host, Address.Port);
      Stream = Client.GetStream();
    }
    catch (Exception e)
    {
      await DisposeTcpConnectionAsync();
      return Result.Fail(e.Message);
    }
    
    var xmppStreamResult = await OpenXmppStream();
    if (xmppStreamResult.IsFailed)
      return xmppStreamResult;
    
    return Result.Ok();
  }

  public Task<Result> DisconnectAsync()
  {
    return (Task<Result>)Task.CompletedTask;
  }

  public Task<Result> ReconnectAsync()
  {
    throw new NotImplementedException();
  }

  #endregion

  #region XMPP Stream Management

  /// <summary>
  /// Opens the XMPP stream via the TCP client
  /// </summary>
  /// <remarks>
  /// Function will return early if there is no XMPP Connection.
  /// Stream will be marked open as soon as the server verifies
  /// </remarks>
  /// <returns>Action result</returns>
  private async Task<Result> OpenXmppStream()
  {
    var validation = ValidateConnection();
    if (validation.IsFailed)
      return validation;

    try
    {
      await Stream!.WriteAsync(Encoding.UTF8.GetBytes("<?xml version='1.0'?>"));
      await Stream.WriteAsync(Encoding.UTF8.GetBytes(
        $"<stream:stream from='{Credentials.Jid}' to='{Address.Host}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>\n"));
      await Stream.FlushAsync();
    }
    catch (Exception e)
    {
      return Result.Fail(e.Message);
    }

    return Result.Ok();
  }

  /// <summary>
  /// Closes the XMPP stream via TCP 
  /// </summary>
  /// <remarks>
  /// Function will return early if there is no XMPP Connection.
  /// Stream will be marked closed as soon as the server verifies
  /// </remarks>
  /// <returns>Action result</returns>
  private async Task<Result> CloseStream()
  {
    var validation = ValidateXmppConnection();
    if (validation.IsFailed)
      return validation;

    await Stream!.WriteAsync(Encoding.UTF8.GetBytes("</stream:stream>\n"));
    await Stream.FlushAsync();

    return Result.Ok();
  }

  #endregion

  #region Stream Features

  public Result RegisterFeature<T>()
  {
    var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
      typeof(T), typeof(XmlRootAttribute));

    if (attr?.Namespace == null)
      return Result.Fail($"Missing feature namespace");
    
    if (FeatureSerializers.ContainsKey(attr.Namespace))
      return Result.Fail($"Namespace {attr.Namespace} already registered");

    FeatureSerializers.Add(attr.Namespace, new XmlSerializer(typeof(T)));
    return Result.Ok();
  }

  public event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureRequested;

  #endregion
  
  #region Background Service

  private async void BackgroundService()
  {
    while (true)
    {
      if (ValidateTcpConnection().IsFailed)
        continue;
    }
  }

  #endregion
}