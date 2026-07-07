using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Backend;
using XMPP.Core.Features;
using XMPP.Core.InfoQueries;
using XMPP.Core.SaslMechanisms;

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
  public required XmppCredentials Credentials { get; init; }
  
  public XmppJid? FullJid { get; set; }
  
  public IXmppClientBackend Backend { get; init; }

  public State XmppState { get; private set; } = State.Disconnected;

  private Stream? _stream;
  private XmlWriter? _writer;

  private Dictionary<string, XmlSerializer> FeatureSerializers { get; } = new();
  private Dictionary<string, XmlSerializer> ErrorSerializers { get; } = new();
  private Dictionary<string, (XmlSerializer, Func<object, object?, Task>)> UnexpectedStanzaSerializers { get; } = new();
  private SortedList<int, ISaslMechanism> SaslHandlers { get; } = new();
  
  private Dictionary<string, TaskCompletionSource<InfoQuery>> InfoQueries { get; } = new();

  private CancellationTokenSource _cts = new();
  private Task? BackgroundServiceHandler { get; set; }

  public void InvokeClientError(IClientError error) =>
    ClientErrorRaisedAsync?.Invoke(this, new StreamErrorEventArgs() {Error = error});

  public SemaphoreSlim ReadLock { get; } = new(1, 1);
  private SemaphoreSlim WriteLock { get; } = new(1, 1);

  private Result ValidateDisconnectedState() => Result.FailIf(XmppState != State.Disconnected,
    "There MUST NOT be an active XMPP connection.");

  private Result ValidateConnectionActiveState() => Result.FailIf(XmppState == State.Disconnected,
    "There MUST be an active XMPP connection.");

  private Result ValidateStreamActiveState() =>
    Result.FailIf(XmppState is State.Disconnected or State.SocketConnected,
      "There MUST be an active XMPP stream. ");

  private Result ValidateStreamValidState() =>
    Result.FailIf(_stream is null,
      "There MUST be an active XMPP stream. ");

  public XmppClient3(IXmppClientBackend backend)
  {
    RegisterFeature<StartTlsFeature>();
    RegisterFeature<SaslFeature>();
    RegisterFeature<BindFeature>();

    RegisterClientError<StreamErrors.BadFormat>();
    RegisterClientError<StreamErrors.BadNamespacePrefix>();
    RegisterClientError<StreamErrors.Conflict>();
    RegisterClientError<StreamErrors.ConnectionTimeout>();
    RegisterClientError<StreamErrors.HostGone>();
    RegisterClientError<StreamErrors.HostUnknown>();
    RegisterClientError<StreamErrors.ImproperAddressing>();
    RegisterClientError<StreamErrors.InternalServerError>();
    RegisterClientError<StreamErrors.InvalidFrom>();
    RegisterClientError<StreamErrors.InvalidNamespace>();
    RegisterClientError<StreamErrors.InvalidXml>();
    RegisterClientError<StreamErrors.NotAuthorized>();
    RegisterClientError<StreamErrors.NotWellFormed>();
    RegisterClientError<StreamErrors.PolicyViolation>();
    RegisterClientError<StreamErrors.RemoteConnectionFailed>();
    RegisterClientError<StreamErrors.Reset>();
    RegisterClientError<StreamErrors.ResourceConstraint>();
    RegisterClientError<StreamErrors.RestrictedXml>();
    RegisterClientError<StreamErrors.SeeOtherHost>();
    RegisterClientError<StreamErrors.SystemShutdown>();
    RegisterClientError<StreamErrors.UndefinedCondition>();
    RegisterClientError<StreamErrors.UnsupportedEncoding>();
    RegisterClientError<StreamErrors.UnsupportedFeature>();
    RegisterClientError<StreamErrors.UnsupportedStanzaType>();
    RegisterClientError<StreamErrors.UnsupportedVersion>();

    RegisterClientError<SaslErrors.Aborted>();
    RegisterClientError<SaslErrors.AccountDisabled>();
    RegisterClientError<SaslErrors.CredentialsExpired>();
    RegisterClientError<SaslErrors.EncryptionRequired>();
    RegisterClientError<SaslErrors.IncorrectEncoding>();
    RegisterClientError<SaslErrors.InvalidAuthZid>();
    RegisterClientError<SaslErrors.InvalidMechanism>();
    RegisterClientError<SaslErrors.MalformedRequest>();
    RegisterClientError<SaslErrors.MechanismTooWeak>();
    RegisterClientError<SaslErrors.NotAuthorized>();
    RegisterClientError<SaslErrors.TemporaryAuthFailure>();
    
    RegisterSaslMechanism<PlainSaslMechanism>();
    RegisterSaslMechanism<ScramSha1SaslMechanism>();
    RegisterSaslMechanism<ScramSha256SaslMechanism>();

    Backend = backend;
    Backend.UseClient(this);
    Backend.NetworkStreamUpdated += OnUpdatedNetworkStream;
    StreamFeatureRequestedAsync += Backend.OnStreamFeatureRequested;
    
    StreamFeatureRequestedAsync += SaslHandler;
    StreamFeatureRequestedAsync += BindHandler;

    ClientErrorRaisedAsync += OnStreamError;
  }

  public async ValueTask DisposeAsync()
  {
    await Disconnect();

    await _cts.CancelAsync();
    if (BackgroundServiceHandler != null) await BackgroundServiceHandler;

    if (_writer != null) await _writer.DisposeAsync();

    Backend.Dispose();
  }

  private void OnUpdatedNetworkStream(object? sender, NetworkStreamUpdatedEventArgs args)
  {
    var stream = args.Stream;

    if (stream is null)
    {
      // Cleanup network stuff
      _writer?.Dispose();
      _stream = null;
      _writer = null;
      return;
    }

    // Re-establish network stuff
    _stream = stream;
    _writer = XmlWriter.Create(_stream, new XmlWriterSettings
    {
      Async = true, CloseOutput = false, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto
    });
  }

  private async void OnStreamError(object? sender, StreamErrorEventArgs args)
  {
    try
    {
      await Disconnect();
    }
    catch (Exception)
    {
      // ignored
    }
  }

  private async void SaslHandler(object? sender, StreamFeatureRequestedEventArgs args)
  {
    if (args.Feature is not SaslFeature sasl)
      return;
    
    Console.WriteLine("Supported SASL mechanisms:");
    sasl.Mechanisms.ForEach(Console.WriteLine);

    foreach (var mechanism in SaslHandlers
               .Where(mechanism
                 => sasl.Mechanisms.Contains(mechanism.Value.Mechanism)))
    {
      Console.WriteLine($"Using P{mechanism.Key}: {mechanism.Value.Mechanism}");
      await mechanism.Value.Use(Credentials);
      break;
    }
  }

  private async void BindHandler(object? sender, StreamFeatureRequestedEventArgs args)
  {
    if (args.Feature is not BindFeature)
      return;
    
    Console.WriteLine($"Binding to resource {Credentials.Jid.Resource}");
    var query = new InfoQuery()
    {
      Type = InfoQueryType.Set,
      ResourceBind = new InfoQuery.Bind()
      {
        Resource = Credentials.Jid.Resource,
      }
    };
    
    var result = await SendInfoQueryAsync(query);
    if (result.IsFailed)
    {
      // todo: throw err
      Console.WriteLine($"Failed to bind to resource {Credentials.Jid.Resource}");
      return;
    }
    
    FullJid = new XmppJid()
    {
      LocalPart = Credentials.Jid.LocalPart,
      DomainPart = Credentials.Jid.DomainPart,
      Resource = Credentials.Jid.Resource,
    };
    
    Console.WriteLine($"XMPP Client Connected to JID {FullJid}");
    
    XmppState = State.Connected;
  }

  public async Task<Result> OpenXmppStream()
  {
    if (ValidateStreamValidState() is { IsFailed: true } r)
      return r;
    
    await WriteLock.WaitAsync();
    await _stream!.WriteAsync("<?xml version='1.0'?>"u8.ToArray());
    await _stream.WriteAsync(Encoding.UTF8.GetBytes(
      $"<stream:stream from='{Credentials.Jid}' to='{Address.Host.TrimEnd(".")}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>\n"));
    await _stream.FlushAsync();
    WriteLock.Release();

    XmppState = State.Negotiating;
    return Result.Ok();
  }

  private async Task CloseXmppStream()
  {
    if (ValidateStreamValidState() is { IsFailed: true })
      return;
    
    Console.WriteLine("Closing XMPP stream");

    await WriteLock.WaitAsync();
    await _stream!.WriteAsync("</stream:stream>"u8.ToArray());
    await _stream.FlushAsync();
    WriteLock.Release();
  }

  public async Task<Result> ConnectAsync()
  {
    if (ValidateDisconnectedState() is { IsFailed: true } r)
      return r;

    await Backend.ConnectAsync(Address);
    XmppState = State.SocketConnected;

    if (await OpenXmppStream() is { IsFailed: true } resultConnect)
      return resultConnect;

    StartBackgroundService();

    return Result.Ok();
  }

  public async Task<Result> Disconnect()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    await StopBackgroundService();

    XmppState = State.Disconnected;
    Backend.Disconnect();

    return Result.Ok();
  }

  public async Task<Result> DisconnectWithStreamCloseAsync()
  {
    await CloseXmppStream();
    return await Disconnect();
  }

  public async Task<Result> ReconnectAsync()
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    if (await DisconnectWithStreamCloseAsync() is { IsFailed: true } resultDisconnect)
      return resultDisconnect;

    if (await ConnectAsync() is { IsFailed: true } resultConnect)
      return resultConnect;

    return Result.Ok();
  }

  public async Task<Result> SendStanzaAsync(object element)
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    await WriteLock.WaitAsync();
    var serializer = new XmlSerializer(element.GetType());
    serializer.Serialize(_writer!, element);
    await _writer!.FlushAsync();
    WriteLock.Release();

    return Result.Ok();
  }
  
  public async Task<Result> SendStanzaAsync(XElement element)
  {
    if (ValidateConnectionActiveState() is { IsFailed: true } r)
      return r;

    await WriteLock.WaitAsync();
    element.WriteTo(_writer!);
    await _writer!.FlushAsync();
    WriteLock.Release();

    return Result.Ok();
  }

  public async Task<Result<InfoQuery>> SendInfoQueryAsync(InfoQuery query)
  {
    var id = Guid.CreateVersion7().ToString();
    query.Id ??= id;
    
    var tcs = new TaskCompletionSource<InfoQuery>();
    InfoQueries[id] = tcs;
    
    if (await SendStanzaAsync(query) is  { IsFailed: true } r)
      return r;
    
    var result = await tcs.Task;
    if (result.Type == InfoQueryType.Error)
      return Result.Fail(result.ToString());
    return Result.Ok(result);
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

  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new()
  {
    var mech = new T();
    mech.BindClient(this);
    SaslHandlers[mech.Priority] = mech;
  }

  public Result RegisterUnexpectedStanza<T>(Func<object, object?, Task> func)
  {
    var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
      typeof(T), typeof(XmlRootAttribute));

    if (attr?.ElementName == null)
      return Result.Fail($"Missing stanza name");
    if (attr.Namespace == null)
      return Result.Fail($"Missing stanza namespace");

    var key = $"{attr.Namespace}/{attr.ElementName}";

    if (UnexpectedStanzaSerializers.ContainsKey(key))
      return Result.Fail($"Stanza with key {key} already registered");

    UnexpectedStanzaSerializers.Add(key, (new XmlSerializer(typeof(T)), func));
    return Result.Ok();
  }

  public Result RegisterClientError<T>() where T : IClientError
  {
    var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
      typeof(T), typeof(XmlRootAttribute));

    if (attr?.ElementName == null)
      return Result.Fail($"Missing error name");
    if (attr.Namespace == null)
      return Result.Fail($"Missing error namespace");

    var key = $"{attr.Namespace}/{attr.ElementName}";

    if (ErrorSerializers.ContainsKey(key))
      return Result.Fail($"Error with key {key} already registered");

    ErrorSerializers.Add(key, new XmlSerializer(typeof(T)));
    return Result.Ok();
  }

  public event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureRequestedAsync;
  public event EventHandler<StreamErrorEventArgs>? ClientErrorRaisedAsync;

  public async Task SaslCompleted()
  {
    await OpenXmppStream();
  }

  public void StartBackgroundService()
  {
    _cts = new CancellationTokenSource();
    BackgroundServiceHandler = Task.Run(BackgroundService);
  }

  public async Task StopBackgroundService()
  {
    await _cts.CancelAsync();
    if (BackgroundServiceHandler != null)
      await BackgroundServiceHandler;
    BackgroundServiceHandler = null;
  }

  private async Task BackgroundService()
  {
    using var reader = XmlReader.Create(_stream!, new XmlReaderSettings
    {
      Async = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true, IgnoreComments = true,
      CloseInput = false
    });
    
    var infoQuerySerializer = new XmlSerializer(typeof(InfoQuery));

    while (!_cts.IsCancellationRequested)
    {
      // Await ReadLock approval
      try
      {
        await ReadLock.WaitAsync(_cts.Token);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      
      await reader.ReadAsync();
      Console.WriteLine($"> {reader.Name}: {reader.NamespaceURI}");

      if (reader.NodeType != XmlNodeType.Element)
      {
        ReadLock.Release();
        continue;
      }

      if (reader.Name == "stream:stream")
      {
        ReadLock.Release();
        continue;
      }

      if (reader.Name == "stream:error")
      {
        using var sub = reader.ReadSubtree();
        await sub.ReadAsync();
        await sub.ReadAsync();

        ErrorSerializers.TryGetValue($"{sub.NamespaceURI}/{sub.Name}", out var errorSerializer);
        var error = errorSerializer?.Deserialize(sub);
        if (error != null)
          ClientErrorRaisedAsync?.Invoke(this, new StreamErrorEventArgs() { Error = (IClientError)error });
        
        break;
      }

      if (reader is { Name: "failure", NamespaceURI: "urn:ietf:params:xml:ns:xmpp-sasl" })
      {
        using var sub = reader.ReadSubtree();
        await sub.ReadAsync();
        await sub.ReadAsync();
        
        ErrorSerializers.TryGetValue($"{sub.NamespaceURI}/{sub.Name}", out var errorSerializer);
        var error = errorSerializer?.Deserialize(sub);
        if (error != null)
          ClientErrorRaisedAsync?.Invoke(this, new StreamErrorEventArgs() { Error = (IClientError)error });
        
        break;
      }

      if (reader.Name == "stream:features")
      {
        using var sub = reader.ReadSubtree();
        await sub.ReadAsync();

        while (await sub.ReadAsync())
          if (sub is { NodeType: XmlNodeType.Element, Depth: 1 })
          {
            Console.WriteLine($"F> {sub.Name}: {sub.NamespaceURI}");
            FeatureSerializers.TryGetValue(sub.NamespaceURI, out var featureSerializer);
            var feature = featureSerializer?.Deserialize(sub);
            if (feature != null)
              StreamFeatureRequestedAsync?.Invoke(this, new StreamFeatureRequestedEventArgs { Feature = feature });
          }

        ReadLock.Release();
        continue;
      }

      if (reader.Name == "iq")
      {
        using var sub = reader.ReadSubtree();
        var infoQuery = (InfoQuery?) infoQuerySerializer.Deserialize(sub);
        if (infoQuery != null)
        {
          InfoQueries.TryGetValue(infoQuery.Id!, out var infoQueryTaskSource);
          infoQueryTaskSource?.TrySetResult(infoQuery);
        }
        
        ReadLock.Release();
        continue;
      }

      {
        using var sub = reader.ReadSubtree();
        var found = UnexpectedStanzaSerializers.TryGetValue($"{reader.NamespaceURI}/{reader.Name}", out var stanzaSerializer);
        
        if (!found)
        {
          ReadLock.Release();
          continue;
        }
        
        var stanza = stanzaSerializer.Item1.Deserialize(sub);
        _ = Task.Run(() => stanzaSerializer.Item2.Invoke(this, stanza));
      }
    }
  }
}
