using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Backend;
using XMPP.Core.Features;

namespace XMPP.Core;

public class XmppClient3 : IXmppClient, IDisposable
{
  public enum State
  {
    Disconnected,
    Negotiating
  }

  public required string Host { get; init; }
  public required XmppCreds Credentials { get; init; }
  public required IXmppClientBackend Backend { get; init; }
  
  public State XmppState { get; private set; } = State.Disconnected;
  
  private NetworkStream? _stream;
  private XmlReader? _reader;
  private XmlWriter? _writer;
  
  private readonly CancellationTokenSource _cts = new();
  private Task BackgroundServiceHandler { get; init; }
  
  private Result ValidateDisconnectedState() => Result.FailIf(XmppState != State.Disconnected, 
    "There MUST NOT be an active XMPP connection.");
  private Result ValidateConnectionActiveState() => Result.FailIf(XmppState == State.Disconnected,
    "There MUST be an active XMPP connection.");

  public XmppClient3()
  {
    BackgroundServiceHandler = Task.Run(BackgroundService);
  }

  public void Dispose()
  {
    _cts.Cancel();
    BackgroundServiceHandler.Wait();
    
    Backend.Dispose();
    
    _reader?.Dispose();
    _writer?.Dispose();
  }

  private void UpdateStreams(NetworkStream? stream)
  {
    if (stream is null)
    {
      _stream =  null;
      _reader = null;
      _writer = null;
      
      return;
    }

    _stream = stream;
    
    _reader = XmlReader.Create(_stream, new XmlReaderSettings
    {
      Async = true, ConformanceLevel = ConformanceLevel.Fragment, CloseInput = false
    });
    
    _writer = XmlWriter.Create(_stream, new XmlWriterSettings
    {
      Async = true, ConformanceLevel = ConformanceLevel.Fragment, CloseOutput = false, OmitXmlDeclaration = true, Encoding = Encoding.UTF8
    });
  }

  private async Task OpenXmppStream() {}
  private async Task CloseXmppStream() {}

  public async Task<Result> ConnectAsync()
  {
    if (ValidateDisconnectedState() is { IsFailed: true } r)
      return r;
    
    await Backend.ConnectAsync(Host);
    UpdateStreams(Backend.Stream);
    
    XmppState = State.Negotiating;
    return Result.Ok();
  }

  public async Task<Result> DisconnectAsync()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    Backend.Disconnect();
    UpdateStreams(Backend.Stream);

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
    throw new NotImplementedException();
  }

  private async Task BackgroundService()
  {
    while (!_cts.IsCancellationRequested)
    {
      if (_reader is null)
      {
        await Task.Delay(100, _cts.Token);
        continue;
      }

      await _reader.ReadAsync();
      
      if (_reader.NodeType != XmlNodeType.Element)
        continue; 
      
      var serializer = new XmlSerializer(typeof(StartTlsFeature));
      var s = serializer.Deserialize(_reader);
      Console.WriteLine(s);
    }
  }
}