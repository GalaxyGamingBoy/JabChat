using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Features;

namespace XMPP.Core;

public class XmppClient(XmppCreds creds) : IXmppClient, IAsyncDisposable
{
  private readonly TcpClient _client = new TcpClient();
  private readonly CancellationTokenSource _cts = new();
  
  private NetworkStream? _stream;
  private XmlWriter? _writer;
  private Task? _backgroundService;

  private XmppCreds Credentials { get; } = creds;
  
  public async ValueTask DisposeAsync()
  {
    await _cts.CancelAsync();
    
    if (_stream != null)
      await _stream.DisposeAsync();
    _client.Dispose();
    
    GC.SuppressFinalize(this);
  }

  public async Task ConnectAsync(XmppAddress address)
  {
    if (Credentials == null) throw new InvalidOperationException("Credentials for XMPP Client cannot be null.");

    // Connect via TCP
    await _client.ConnectAsync(address.Ip, address.Port);

    _stream = _client.GetStream();
    _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

    // Start XML Stream
    await _stream.WriteAsync(Encoding.UTF8.GetBytes("<?xml version='1.0'?>"));
    await _stream.WriteAsync(Encoding.UTF8.GetBytes(
      $"<stream:stream from='{Credentials.Jid}' to='{address.Host}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>"));
    await _stream.FlushAsync();

    // Create XML Writer
    _writer = XmlWriter.Create(_stream,
      new XmlWriterSettings
      {
        Async = true, ConformanceLevel = ConformanceLevel.Fragment, OmitXmlDeclaration = true, CloseOutput = false,
        Encoding = Encoding.UTF8
      });

    // Start Background Processing
    _backgroundService = Task.Run(BackgroundService);
  }

  public Task<Result> ConnectAsync()
  {
    throw new NotImplementedException();
  }

  public Task<Result> DisconnectAsync()
  {
    throw new NotImplementedException();
  }

  public Task<Result> ReconnectAsync()
  {
    throw new NotImplementedException();
  }

  public Task StartBackgroundServiceAsync()
  {
    throw new NotImplementedException();
  }


  public Task StopBackgroundServiceAsync()
  {
    throw new NotImplementedException();
  }

  public Result RegisterFeature<T>()
  {
    throw new NotImplementedException();
  }

  public void RegisterFeature<T>(string ns) where T : IXmlSerializable
  {
    throw new NotImplementedException();
  }

  public Result RegisterFeature()
  {
    throw new NotImplementedException();
  }

  private async Task BackgroundService()
  {
    if (_stream == null) return;

    using var reader = XmlReader.Create(_stream,
      new XmlReaderSettings() { Async = true, ConformanceLevel = ConformanceLevel.Fragment });

    while (!_cts.IsCancellationRequested)
    {
      if (!await reader.ReadAsync()) break;
      
      if (reader.NodeType != XmlNodeType.Element)
        continue;
      
      var serializer = new XmlSerializer(typeof(StartTlsFeature));
      serializer.Deserialize(reader);

      // todo: more parsing...
    }
  }
}