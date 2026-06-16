using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Backend;
using XMPP.Core.Features;

namespace XMPP.Core;

public class XmppClient3 : IXmppClient, IAsyncDisposable
{
  /// <summary>
  /// XMPP Client State
  /// </summary>
  public enum State
  {
    /// <summary>
    /// There is no active socket connection to an XMPP host
    /// </summary>
    Disconnected,
    /// <summary>
    /// There is an active socket connection to an XMPP host, but stream negotiation hasn't started yet
    /// </summary>
    SocketConnected,
    /// <summary>
    /// There is an active XMPP stream negotiation
    /// </summary>
    Negotiating,
    /// <summary>
    /// The XMPP stream has been established, no actions pending
    /// </summary>
    Connected,
  }

  public required XmppAddress Address { get; init; }
  public required XmppCreds Credentials { get; init; }
  public IXmppClientBackend Backend { get; init; }
  
  public State XmppState { get; private set; } = State.Disconnected;
  
  private NetworkStream? _stream;
  private XmlReader? _reader;
  private XmlWriter? _writer;
  
  private Dictionary<string, XmlSerializer> FeatureSerializers { get; } = new();
  
  private readonly CancellationTokenSource _cts = new();
  private Task BackgroundServiceHandler { get; init; }
  
  private Result ValidateDisconnectedState() => Result.FailIf(XmppState != State.Disconnected, 
    "There MUST NOT be an active XMPP connection.");
  private Result ValidateConnectionActiveState() => Result.FailIf(XmppState == State.Disconnected,
    "There MUST be an active XMPP connection.");
  private Result ValidateStreamActiveState() =>
    Result.FailIf(XmppState is State.Disconnected or State.SocketConnected, 
      "There MUST be an active XMPP stream. ");

  public XmppClient3(IXmppClientBackend backend)
  {
    RegisterFeature<StartTlsFeature>();
    RegisterFeature<SaslFeature>();
    RegisterFeature<BindFeature>();
    
    Backend = backend;
    Backend.NetworkStreamUpdated += OnUpdatedNetworkStream;
    StreamFeatureRequested += Backend.OnStreamFeatureRequested;
    
    BackgroundServiceHandler = Task.Run(BackgroundService);
  }

  public async ValueTask DisposeAsync()
  {
    await DisconnectAsync();
    
    await _cts.CancelAsync();
    await BackgroundServiceHandler;
    
    Backend.Dispose();
    
    // Reader, Writer, Stream disposal in Backend & OnUpdatedNetworkStream
  }

  public void Dispose()
  {
    DisconnectAsync().Wait();
    
    _cts.Cancel();
    BackgroundServiceHandler.Wait();
    
    Backend.Dispose();
    
    _reader?.Dispose();
    _writer?.Dispose();
  }

  private void OnUpdatedNetworkStream(object? sender, NetworkStreamUpdatedEventArgs args)
  {
    var stream = args.Stream;
    
    if (stream is null)
    {
      _reader?.Dispose();
      _writer?.Dispose();
      
      _stream =  null;
      _reader = null;
      _writer = null;
      
      return;
    }
    
    _stream = stream;
    
    _reader = XmlReader.Create(_stream, new XmlReaderSettings
    {
      Async = true, IgnoreProcessingInstructions =  true, IgnoreWhitespace = true,  IgnoreComments = true
    });
    
    _writer = XmlWriter.Create(_stream, new XmlWriterSettings
    {
      Async = true, CloseOutput = false, OmitXmlDeclaration = true
    });   
  }

  private async Task<Result> OpenXmppStream()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;
    
    await _stream!.WriteAsync(Encoding.UTF8.GetBytes("<?xml version='1.0'?>"));
    await _stream.WriteAsync(Encoding.UTF8.GetBytes(
      $"<stream:stream from='{Credentials.Jid}' to='{Address.Host.TrimEnd(".")}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>\n"));
    await _stream.FlushAsync();

    XmppState = State.Negotiating;
    return Result.Ok();
  }

  private async Task CloseXmppStream()
  {
    throw new NotImplementedException();
  }

  public async Task<Result> ConnectAsync()
  {
    if (ValidateDisconnectedState() is { IsFailed: true } r)
      return r;
    
    await Backend.ConnectAsync(Address);
    XmppState = State.SocketConnected;
    
    if (await OpenXmppStream() is { IsFailed: true } resultConnect)
      return resultConnect;
    
    return Result.Ok();
  }

  public async Task<Result> DisconnectAsync()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    Backend.Disconnect();

    XmppState = State.Disconnected;
    return Result.Ok();
  }

  public async Task<Result> ReconnectAsync()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    if (await DisconnectAsync() is { IsFailed: true } resultDisconnect)
      return resultDisconnect;
    
    if (await ConnectAsync() is { IsFailed: true } resultConnect)
      return resultConnect;
    
    return Result.Ok();
  }

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

  private async Task BackgroundService()
  {
    while (!_cts.IsCancellationRequested)
    {
      if (ValidateStreamActiveState()  is { IsFailed: true })
      {
        await Task.Delay(100, _cts.Token);
        continue;
      }
      
      await _reader!.ReadAsync();
      
      if (_reader.NodeType != XmlNodeType.Element)
        continue; 
     
      Console.WriteLine($"{_reader.Name}: {_reader.NamespaceURI}");
      
      if (_reader.Name == "stream:stream")
        continue; // todo: store stream from server

      if (_reader.Name == "stream:features")
      {
        using var sub = _reader.ReadSubtree();
        await sub.ReadAsync();
        
        while (await sub.ReadAsync())
          if (sub is { NodeType: XmlNodeType.Element, Depth: 1 })
          {
            FeatureSerializers.TryGetValue(sub.NamespaceURI, out var featureSerializer);
            if (featureSerializer == null) continue;
            var feature = featureSerializer.Deserialize(sub);
            if (feature == null) continue;
            StreamFeatureRequested?.Invoke(this, new StreamFeatureRequestedEventArgs { Feature = feature });
          }
      }
      
      // todo: proc
    }
  }
}