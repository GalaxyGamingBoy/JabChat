using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using OneOf;
using XMPP.Core.Address;
using XMPP.Core.Backend;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.Features;
using XMPP.Core.IM;
using XMPP.Core.InfoQueries;
using XMPP.Core.LogMessages;
using XMPP.Core.Messages;
using XMPP.Core.SaslMechanisms;
using XmppClientLogs = XMPP.Core.LogMessages.XmppClientLogs;

namespace XMPP.Core;

using ConnectResult = OneOf<
  Unit,
  ConnectResults.AddressPortInvalid,
  ConnectResults.ClientAlreadyConnected,
  ConnectResults.ConnectionFailure
>;

using DisconnectResult = OneOf<
  Unit,
  DisconnectResults.StreamNullException,
  DisconnectResults.AlreadyDisconnected
>;

using ReconnectResult = OneOf<
  Unit,
  ReconnectResults.AddressPortInvalid,
  ReconnectResults.ClientAlreadyConnected,
  ReconnectResults.ReconnectionFailure
>;

using OpenXmppStreamResult = OneOf<
  Unit,
  OpenXmppStreamResults.StreamNullException
>;

using CloseXmppStreamResult = OneOf<
  Unit,
  CloseXmppStreamResults.StreamNullException
>;

using SendStanzaResult = OneOf<
  Unit,
  SendStanzaResults.SerializationFailure,
  SendStanzaResults.WriterNullException
>;

using SendInfoQueryResult = OneOf<
  InfoQuery,
  SendInfoQueryResults.InfoQueryError,
  SendInfoQueryResults.SerializationFailure,
  SendInfoQueryResults.WriterNullException
>;

using SendPresenceResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using SendMessageResult = OneOf<
  Unit,
  SendMessageResults.SerializationFailure,
  SendMessageResults.WriterNullException
>;

using RegisterUnexpectedStanzaResult = OneOf<
  Unit,
  RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered
>;

public class XmppClient(int maxExtensionLength = 32) : IXmppClient, IAsyncDisposable
{
  #region Internal Fields

  private Stream? _stream;

  private XmlWriter? _writer;
  
  private readonly Dictionary<string, TaskCompletionSource<InfoQuery>> _infoQueries = new();
  
  private readonly SortedList<int, ISaslMechanism> _saslHandlers = new();
  
  private readonly Dictionary<string, (XmlSerializer, Func<object, object?, Task>)> _unexpectedStanzaSerializers = new();
  
  private CancellationTokenSource _backgroundServiceTokenSource = new();

  private Task? _backgroundServiceHandler;
  
  private XmppJid? _fullJid;

  private readonly IXmppClientBackend _backend = null!;
  
  private readonly BitArray _enabledExtensions = new(maxExtensionLength);
  private readonly Dictionary<int, IXmppClientExtension> _extensions = new();
  
  private ILogger<XmppClient> _logger = JabChatLogging.Factory.CreateLogger<XmppClient>();
  
  #endregion

  #region Public Fields
  
  public XmppState State { get; set; } = XmppState.Disconnected;

  public required XmppAddress Address { get; init; }
  
  public required XmppCredentials Credentials { get; init; }

  public XmppJid ConnectedJid => _fullJid ?? Credentials.Jid;

  #endregion

  public XmppClient(IXmppClientBackend backend) : this()
  {
    XmppClientLogs.ConfiguringClient(_logger);
    
    // Backend Configuration
    XmppClientLogs.BindingBackend(_logger);
    _backend = backend;
    _backend.UseClient(this);
    _backend.NetworkStreamUpdated += OnUpdatedNetworkStream;
    StreamFeatureAdvertised += _backend.OnStreamFeatureAdvertised;

    // Sasl Mechanisms
    RegisterSaslMechanism<PlainSaslMechanism>();

    RegisterSaslMechanism<ScramSha1SaslMechanism>();
    RegisterSaslMechanism<ScramSha256SaslMechanism>();
    RegisterSaslMechanism<ScramSha384SaslMechanism>();
    RegisterSaslMechanism<ScramSha512SaslMechanism>();
    RegisterSaslMechanism<ScramSha3512SaslMechanism>();

    RegisterSaslMechanism<ScramSha1PlusSaslMechanism>();
    RegisterSaslMechanism<ScramSha256PlusSaslMechanism>();
    RegisterSaslMechanism<ScramSha384PlusSaslMechanism>();
    RegisterSaslMechanism<ScramSha512PlusSaslMechanism>();
    RegisterSaslMechanism<ScramSha3512PlusSaslMechanism>();

    // Internal Handlers
    StreamFeatureAdvertised += SaslHandler;
    StreamFeatureAdvertised += BindHandler;
    ClientErrorRaised += OnStreamError;
    
    // Enable Extensions
    EnableExtension<ImExtension>();
    
    XmppClientLogs.CompletedClientConfiguration(_logger);
  }
  
  public async ValueTask DisposeAsync()
  {
    XmppClientLogs.DisposingClient(_logger);
    
    await DisconnectWithStreamCloseAsync();

    await _backgroundServiceTokenSource.CancelAsync();
    if (_backgroundServiceHandler != null) await _backgroundServiceHandler;
    
    _backend.NetworkStreamUpdated -= OnUpdatedNetworkStream;
    StreamFeatureAdvertised -= _backend.OnStreamFeatureAdvertised;

    if (_writer != null) await _writer.DisposeAsync();
    
    _backend.Dispose();
  }
  
  #region Connection Management

  public async Task<ConnectResult> ConnectAsync()
  {
    if (State != XmppState.Disconnected)
      return new ConnectResults.ClientAlreadyConnected();
    
    XmppClientLogs.Connecting(_logger, Address.Host);
    
    var backendConnectResult = await _backend.ConnectAsync(Address);
    if (!backendConnectResult.IsT0)
      return backendConnectResult.Match<ConnectResult>(
        _ => throw new UnreachableException(),
        _ => new ConnectResults.AddressPortInvalid(), 
        _ => new ConnectResults.ClientAlreadyConnected(),
        _ => new ConnectResults.ConnectionFailure());
    
    State = XmppState.SocketConnected;
    
    var streamResult = await OpenXmppStream();
    if (!streamResult.IsT0)
      return streamResult.Match<ConnectResult>(
        _ => throw new UnreachableException(),
        _ => new ConnectResults.ClientAlreadyConnected());
    
    StartBackgroundService();
    
    await Task.WhenAll(_extensions.Values.Select(async t => await t.OnSocketConnected()));

    return new Unit();
  }

  public async Task<DisconnectResult> Disconnect()
  {
    if (State == XmppState.Disconnected)
      return new DisconnectResults.AlreadyDisconnected();
    
    XmppClientLogs.DisposingClient(_logger);
    
    await Task.WhenAll(_extensions.Values.Select(async t => await t.OnDisconnected()));

    await StopBackgroundService();

    State = XmppState.Disconnected;
    _backend.Disconnect();

    return new Unit();
  }
  
  public async Task<DisconnectResult> DisconnectWithStreamCloseAsync()
  {
    var closeResult = await CloseXmppStream();
    if (!closeResult.IsT0)
      return closeResult.Match<DisconnectResult>(
        _ => new Unit(),
        _ =>  new DisconnectResults.StreamNullException());
    
    return await Disconnect();
  }
  
  public async Task<ReconnectResult> ReconnectAsync()
  {
    if (State != XmppState.Disconnected)
      return new ReconnectResults.ClientAlreadyConnected();
    
    XmppClientLogs.Reconnecting(_logger, Address.Host);

    await DisconnectWithStreamCloseAsync();

    var connectResult = await ConnectAsync();
    if (!connectResult.IsT0)
      connectResult.Match<ReconnectResult>(
        _ => new Unit(),
        _ => new ReconnectResults.AddressPortInvalid(),
        _ => new ReconnectResults.ClientAlreadyConnected(),
        _ => new ReconnectResults.ReconnectionFailure());

    return new Unit();
  }
  
  #endregion

  #region Message Management

  public SemaphoreSlim ReadLock { get; } = new(1, 1);
  
  private SemaphoreSlim WriteLock { get; } = new(1, 1);
  
  public async Task<SendStanzaResult> SendStanzaAsync(object element)
  {
    if (_writer is null)
      return new SendStanzaResults.WriterNullException();

    await WriteLock.WaitAsync();
    var serializer = new XmlSerializer(element.GetType());

    try
    {
      serializer.Serialize(_writer!, element);
    }
    catch (InvalidOperationException)
    {
      return new SendStanzaResults.SerializationFailure();
    }
    
    await _writer!.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }

  public async Task<SendStanzaResult> SendStanzaAsync(XElement element)
  {
    if (_writer is null)
      return new SendStanzaResults.WriterNullException();

    await WriteLock.WaitAsync();
    element.WriteTo(_writer);
    await _writer.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }
  
  public async Task<SendInfoQueryResult> SendInfoQueryAsync(InfoQuery query)
  {
    var id = Guid.CreateVersion7().ToString();
    query.Id ??= id;
    
    XmppClientLogs.SendingInfoQuery(_logger, id);

    var tcs = new TaskCompletionSource<InfoQuery>(TaskCreationOptions.RunContinuationsAsynchronously);
    _infoQueries[id] = tcs;
    
    var stanzaResult = await SendStanzaAsync(query);
    if (!stanzaResult.IsT0)
      return stanzaResult.Match<SendInfoQueryResult>(
        _ => throw new UnreachableException(),
        _ => new SendInfoQueryResults.SerializationFailure(),
        _ => new SendInfoQueryResults.WriterNullException());
      
    var result = await tcs.Task;
    XmppClientLogs.ReceivedInfoQueryResult(_logger, id);
    
    if (result.Type == InfoQueryType.Error)
      return new SendInfoQueryResults.InfoQueryError(result.ToString(), result.StanzaError!);
    return result;
  }

  public async Task<SendPresenceResult> SendPresenceAsync(Presence.Presence presence)
  {
    var id =  Guid.CreateVersion7().ToString();
    presence.Id ??= id;
    
    XmppClientLogs.SendingPresence(_logger, id);

    return (await SendStanzaAsync(presence)).Match<SendPresenceResult>(
      _ => new Unit(),
      _ => new SendPresenceResults.SerializationFailure(),
      _ => new SendPresenceResults.WriterNullException());
  }

  public async Task<SendMessageResult> SendMessageAsync(Message message)
  {
    var id = Guid.CreateVersion7().ToString();
    message.Id ??= id;
    
    XmppClientLogs.SendingMessage(_logger, id);
    
    return (await SendStanzaAsync(message)).Match<SendMessageResult>(
      _ => new Unit(),
      _ => new SendMessageResults.SerializationFailure(),
      _ => new SendMessageResults.WriterNullException());
  }

  #endregion

  #region Element Registrations

  public RegisterUnexpectedStanzaResult RegisterUnexpectedStanza<T>(Func<object, object?, Task> func)
    where T : IXmppStanzaKey<T>
  {
    var key = T.ToStanzaKey();
    if (_unexpectedStanzaSerializers.ContainsKey(key))
    {
      XmppClientLogs.ConflictingUnexpectedStanza(_logger, key);
      return new RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered(key);
    }

    _unexpectedStanzaSerializers.Add(key, (new XmlSerializer(typeof(T)), func));
    XmppClientLogs.RegisteredUnexpectedStanza(_logger, key);
    return new Unit();
  }
  
  public void UnregisterUnexpectedStanza<T>()
    where T : IXmppStanzaKey<T>
  {
    var key = T.ToStanzaKey();
    _unexpectedStanzaSerializers.Remove(key);
    XmppClientLogs.UnregisteredUnexpectedStanza(_logger, key);
  }
  
  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new()
  {
    var mech = new T();
    mech.BindClient(this, _backend);
    _saslHandlers[mech.Priority] = mech;
    XmppClientLogs.RegisteredSaslMechanism(_logger, mech.Mechanism, mech.Priority);
  }
  
  #endregion

  #region Stream Management

  public async Task<OpenXmppStreamResult> OpenXmppStream()
  {
    if (_stream is null)
      return new OpenXmppStreamResults.StreamNullException();
    
    XmppClientLogs.OpeningXmppStream(_logger);

    var to = Address.Host.TrimEnd(".").ToString();

    await WriteLock.WaitAsync();
    _stream!.Write("<?xml version='1.0'?>"u8.ToArray());
    _stream.Write(Encoding.UTF8.GetBytes(
      $"<stream:stream from='{Credentials.Jid}' to='{to}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>\n"));
    await _stream.FlushAsync();
    WriteLock.Release();

    State = XmppState.Negotiating;
    return new Unit();
  }
  
  private async Task<CloseXmppStreamResult> CloseXmppStream()
  {
    if (_stream is null)
      return new CloseXmppStreamResults.StreamNullException();
    
    XmppClientLogs.ClosingXmppStream(_logger);

    await WriteLock.WaitAsync();
    await _stream!.WriteAsync("</stream:stream>"u8.ToArray());
    await _stream.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }

  #endregion

  #region Readers

  private async Task ReadStreamFeatures(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync();
    await sub.ReadAsync(); // Skip Header

    while (sub is not {NodeType: XmlNodeType.EndElement, LocalName: "features"})
      if (sub is { NodeType: XmlNodeType.Element, Depth: 1 })
      {
        var key = $"{sub.NamespaceURI}/{sub.LocalName}";
        XmppClientLogs.ReadStreamFeature(_logger, key);
        
        XmppClientRegistry.FeatureSerializers.TryGetValue(key, out var featureSerializer);
        var feature = featureSerializer?.Deserialize(sub);
        if (feature != null)
          StreamFeatureAdvertised?.Invoke(this, new StreamFeatureRequestedEventArgs { Feature = feature });
        else
          await sub.SkipAsync();
      }

    ReadLock.Release();
  }
  
  private readonly XmlSerializer _infoQuerySerializer = new(typeof(InfoQuery));

  private async Task ReadInfoQuery(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync();
    
    var infoQuery = (InfoQuery?)_infoQuerySerializer.Deserialize(sub);
    if (infoQuery == null)
      return;

    infoQuery.StanzaError?.Errors = ParseStanzaErrors(infoQuery.StanzaError.InternalErrors);
    infoQuery.DeserializeExtensions();
    
    ReadLock.Release();
    
    _infoQueries.TryGetValue(infoQuery.Id!, out var infoQueryTaskSource);
    if (infoQueryTaskSource is null)
      OnUnexpectedInfoQueryReceived?.Invoke(this,
        new OnUnexpectedInfoQueryReceivedEventArgs() { InfoQuery = infoQuery });
    else
      infoQueryTaskSource.TrySetResult(infoQuery);
  }
  
  private readonly XmlSerializer _messageSerializer = new(typeof(Message));

  private async Task ReadMessage(XmlReader reader)
  {
    ReadLock.Release();
    
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync();
    
    var message = (Message?)_messageSerializer.Deserialize(sub);
    if (message == null) return;

    message.StanzaError?.Errors = ParseStanzaErrors(message.StanzaError.InternalErrors);
    message.DeserializeExtensions();

    OnMessageReceived?.Invoke(this, new OnMessageReceivedEventArgs { Message = message });
  }

  private readonly XmlSerializer _presenceSerializer = new(typeof(Presence.Presence));

  private async Task ReadPresence(XmlReader reader)
  {
    ReadLock.Release();
    
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync();
    
    var presence = (Presence.Presence?)_presenceSerializer.Deserialize(sub);
    if (presence == null) return;
    
    presence.StanzaError?.Errors = ParseStanzaErrors(presence.StanzaError.InternalErrors);
    presence.DeserializeExtensions();
    
    OnPresenceReceived?.Invoke(this, new OnPresenceReceivedEventArgs { Presence = presence });
  }

  private void ReadUnexpectedStanza(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    var found = _unexpectedStanzaSerializers.TryGetValue($"{reader.NamespaceURI}/{reader.Name}",
      out var stanzaSerializer);

    if (!found)
    {
      ReadLock.Release();
      return;
    }

    var stanza = stanzaSerializer.Item1.Deserialize(sub);
    _ = Task.Run(() => stanzaSerializer.Item2.Invoke(this, stanza));
  }

  private async Task<IClientError?> ReadSingleError(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync(); 
    await sub.ReadAsync(); // Skip header

    XmppClientRegistry.ErrorSerializers.TryGetValue($"{sub.NamespaceURI}/{sub.LocalName}", out var errorSerializer);
    return errorSerializer?.Deserialize(sub) as IClientError;
  }

  #endregion
  
  #region Background Service

  public void StartBackgroundService()
  {
    XmppClientLogs.StartBackgroundService(_logger);
    _backgroundServiceTokenSource = new CancellationTokenSource();
    _backgroundServiceHandler = Task.Run(BackgroundService);
  }

  public async Task StopBackgroundService()
  {
    XmppClientLogs.StopBackgroundService(_logger);
    await _backgroundServiceTokenSource.CancelAsync();
    if (_backgroundServiceHandler != null)
      await _backgroundServiceHandler;
    _backgroundServiceHandler = null;
  }
  
  private async Task BackgroundService()
  {
    using var reader = XmlReader.Create(_stream!, new XmlReaderSettings
    {
      Async = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true, IgnoreComments = true,
      CloseInput = false
    });

    while (!_backgroundServiceTokenSource.IsCancellationRequested)
    {
      // await ReadLock approval
      try
      {
        XmppClientLogs.ReaderWaitingReadLock(_logger);
        await ReadLock.WaitAsync(_backgroundServiceTokenSource.Token);
      }
      catch (OperationCanceledException)
      {
        break;
      }

      XmppClientLogs.ReaderPassBegun(_logger);
      await reader.ReadAsync();
      XmppClientLogs.ReadXmlElement(_logger, reader.NamespaceURI, reader.Name);

      if (reader.NodeType != XmlNodeType.Element)
      {
        XmppClientLogs.NonXmlElementReadSkipping(_logger);
        ReadLock.Release();
        continue;
      }
      
      switch (reader.Name)
      {
        case "stream:stream":
          XmppClientLogs.EncounteredStreamHeader(_logger);
          ReadLock.Release();
          break;
        case "stream:features":
          XmppClientLogs.ReadStreamFeatureBegun(_logger);
          await ReadStreamFeatures(reader);
          XmppClientLogs.ReadStreamFeatureEnded(_logger);
          break;
        case "stream:error":
          XmppClientLogs.EncounteredStreamError(_logger);
          InvokeClientError(await ReadSingleError(reader) ?? new GenericError());
          break;
        case "failure":
          if (reader.NamespaceURI == "urn:ietf:params:xml:ns:xmpp-sasl")
          {
            XmppClientLogs.EncounteredSaslError(_logger);
            InvokeClientError(await ReadSingleError(reader) ?? new GenericError());
          }
          break;
        case "iq":
          XmppClientLogs.ReadInfoQueryBegun(_logger);
          await ReadInfoQuery(reader);
          XmppClientLogs.ReadInfoQueryEnded(_logger);
          break;
        case "message":
          XmppClientLogs.ReadMessageBegun(_logger);
          await ReadMessage(reader);
          XmppClientLogs.ReadMessageEnded(_logger);
          break;
        case "presence":
          XmppClientLogs.ReadPresenceBegun(_logger);
          await ReadPresence(reader);
          XmppClientLogs.ReadPresenceEnded(_logger);
          break;
        default:
          XmppClientLogs.ReadUnexpectedStanzaBegun(_logger);
          ReadUnexpectedStanza(reader);
          XmppClientLogs.ReadUnexpectedStanzaEnded(_logger);
          break;
      }
      
      XmppClientLogs.ReaderPassEnded(_logger);
    }
  }
  
  #endregion
  
  #region Extensions

  public bool IsExtensionEnabled(int extensionIdentifier)
  {
    return _enabledExtensions[extensionIdentifier];
  }

  public bool IsExtensionEnabled<T>() where T : class, IXmppClientExtension<T>
  {
    return IsExtensionEnabled(T.ExtensionIdentifier);
  }

  public bool AreExtensionsEnabled(BitArray extensions)
  {
    var enabled = new BitArray(_enabledExtensions);
    enabled.And(extensions);
    return enabled.Equals(extensions);
  }

  public void EnableExtension<T>() where T : class, IXmppClientExtension<T>
  {
    XmppClientLogs.EnablingExtension(_logger, typeof(T).FullName, T.ExtensionIdentifier);
    var extension = T.Create(this);
    _extensions[T.ExtensionIdentifier] = extension;
    
    _enabledExtensions[T.ExtensionIdentifier] = true;

    _ = Task.Run(async () =>
    {
      await Task.WhenAll(_extensions.Values.Select(async t => await t.OnEnable()));
    });
  }

  public async Task DisableExtension<T>() where T : class, IXmppClientExtension<T>
  {
    XmppClientLogs.DisablingExtension(_logger, typeof(T).FullName, T.ExtensionIdentifier);
    
    var ext = GetExtension<T>();
    if (ext != null) {
      await ext.DisposeAsync();
    }
    
    _extensions.Remove(T.ExtensionIdentifier);
    _enabledExtensions[T.ExtensionIdentifier] = false;
  }

  public T? GetExtension<T>() where T : class, IXmppClientExtension<T>
  {
    if (!_enabledExtensions[T.ExtensionIdentifier])
      return null;

    return _extensions[T.ExtensionIdentifier] as T;
  }
  #endregion
  
  #region Error Handling

  public event EventHandler<ClientErrorRaisedEventArgs>? ClientErrorRaised;
  
  public void InvokeClientError(IClientError error) =>
    ClientErrorRaised?.Invoke(this, new ClientErrorRaisedEventArgs() { Error = error });

  private async void OnStreamError(object? sender, ClientErrorRaisedEventArgs args)
  {
    try
    {
      await Disconnect();

      await StopBackgroundService();
      StartBackgroundService();
    }
    catch (Exception)
    {
      // ignored
    }
  }
  
  private List<IClientError> ParseStanzaErrors(List<XmlElement> errors)
  {
    var parsed =
      errors.Select(element =>
      {
        XmppClientRegistry.ErrorSerializers.TryGetValue($"{element.NamespaceURI}/{element.Name}", out var errorSerializer);
        using var reader = new XmlNodeReader(element);
        return (IClientError?)errorSerializer?.Deserialize(reader);
      }).Where(e => e != null).ToList();

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    return (List<IClientError>)parsed;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
  }
  
  #endregion
  
  #region Event Handlers and Callbacks

  public event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureAdvertised;
  
  public event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;
  
  public event EventHandler<OnPresenceReceivedEventArgs>? OnPresenceReceived;
  
  public event EventHandler<OnUnexpectedInfoQueryReceivedEventArgs>? OnUnexpectedInfoQueryReceived;
  
  public async Task SaslCompleted()
  {
    XmppClientLogs.SaslAuthenticationEnded(_logger);
    // The SaslCompleted Handler, opens the new XmppStream that advertises Resource Binding
    await Task.WhenAll(_extensions.Values.Select(async t => await t.AfterSasl()));
    await OpenXmppStream();
  }
  
  private async void SaslHandler(object? sender, StreamFeatureRequestedEventArgs args)
  {
    try
    {
      if (args.Feature is not SaslFeature sasl)
        return;
      
      XmppClientLogs.SaslAuthenticationBegun(_logger);

      await Task.WhenAll(_extensions.Values.Select(async t => await t.BeforeSasl()));

      XmppClientLogs.SaslServerSupports(_logger, string.Join(", ", sasl.Mechanisms));

      var commonMechanisms = _saslHandlers
        .Where(mech => sasl.Mechanisms.Contains(mech.Value.Mechanism));
      
      var mechanism = commonMechanisms.First(); 
      XmppClientLogs.UsingSaslMechanism(_logger, mechanism.Value.Mechanism, mechanism.Key);
      await mechanism.Value.Use(Credentials);
    }
    catch (Exception)
    {
      // ignored
    }
  }

  private async void BindHandler(object? sender, StreamFeatureRequestedEventArgs args)
  {
    try
    {
      if (args.Feature is not BindFeature)
        return;
      
      await Task.WhenAll(_extensions.Values.Select(async t => await t.BeforeBind()));

      var resource = Credentials.Jid.Resource ?? Guid.NewGuid().ToString();
      XmppClientLogs.BindResourceBegun(_logger, resource);
      
      var query = new InfoQuery()
      {
        Type = InfoQueryType.Set,
        ResourceBind = new Bind()
        {
          Resource = resource,
        }
      };

      var result = await SendInfoQueryAsync(query);
      await Task.WhenAll(_extensions.Values.Select(async t => await t.AfterBind()));
      if (result.IsT1)
      {
        var errors = result.AsT1.StanzaError.Errors.Select(e => e.What());
        InvokeClientError(new BindError(resource, string.Join(Environment.NewLine, errors)));
        return;
      }
      
      if (!result.IsT0)
      {
        InvokeClientError(new BindError(resource, (result.Value as IClientError)?.What()!));
        return;
      }
      
      var iq = result.AsT0;
      _fullJid = Credentials.Jid with { Resource = iq.ResourceBind!.Resource };
      XmppClientLogs.BindResourceEnded(_logger, iq.ResourceBind!.Resource);
      XmppClientLogs.Connected(_logger, _fullJid);

      State = XmppState.Connected;
      await Task.WhenAll(_extensions.Values.Select(async t => await t.OnConnected()));
    }
    catch (Exception)
    {
      // ignored
    }
  }
  
  private void OnUpdatedNetworkStream(object? sender, NetworkStreamUpdatedEventArgs args)
  {
    // todo: no need to set stream to null first - check actual functionality
    var stream = args.Stream;
    
    XmppClientLogs.NetworkStreamUpdated(_logger);

    _writer?.Dispose();
    _stream = stream;

    if (_stream is not null)
      _writer = XmlWriter.Create(_stream, new XmlWriterSettings
      {
        Async = true, CloseOutput = false, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto
      });
  }
  
  #endregion
}