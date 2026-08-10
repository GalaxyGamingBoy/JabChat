using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using OneOf;
using XMPP.Core.Address;
using XMPP.Core.Backend;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.Features;
using XMPP.Core.IM;
using XMPP.Core.InfoQueries;
using XMPP.Core.Messages;
using XMPP.Core.SaslMechanisms;

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

using RegisterUnexpectedStanzaResult = OneOf<
  Unit,
  RegisterUnexpectedStanzaResults.AmbiguousAttributeMatch,
  RegisterUnexpectedStanzaResults.StanzaNameMissing,
  RegisterUnexpectedStanzaResults.StanzaNamespaceMissing,
  RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered
>;

using UnregisterUnexpectedStanzaResult = OneOf<
  Unit,
  UnregisterUnexpectedStanzaResults.AmbiguousAttributeMatch,
  UnregisterUnexpectedStanzaResults.StanzaNameMissing,
  UnregisterUnexpectedStanzaResults.StanzaNamespaceMissing
>;

public class XmppClient(int maxExtensionLength = 32) : IXmppClient, IAsyncDisposable
{
  #region Internal Fields

  private Stream? Stream { get; set; }
  
  private XmlWriter? Writer {  get; set; }
  
  private Dictionary<string, TaskCompletionSource<InfoQuery>> InfoQueries { get; } = new();
  
  private SortedList<int, ISaslMechanism> SaslHandlers { get; } = new();
  
  private Dictionary<string, (XmlSerializer, Func<object, object?, Task>)> UnexpectedStanzaSerializers { get; } = new();
  
  private CancellationTokenSource _backgroundServiceTokenSource = new();
  
  private Task? BackgroundServiceHandler { get; set; }
  
  private XmppJid? FullJid { get; set; }

  private IXmppClientBackend Backend { get; init; }
  
  private BitArray _enabledExtensions = new(maxExtensionLength);
  private BitArray[] _loadExtensionAt = Enumerable.Repeat(new BitArray(maxExtensionLength), 2).ToArray();
  private Dictionary<int, IXmppClientExtension> _extensions = new();
  
  #endregion

  #region Public Fields
  
  public XmppState State { get; set; } = XmppState.Disconnected;

  public required XmppAddress Address { get; init; }
  
  public required XmppCredentials Credentials { get; init; }

  public XmppJid ConnectedJid => FullJid ?? Credentials.Jid;

  #endregion

  public XmppClient(IXmppClientBackend backend) : this()
  {
    // Backend Configuration
    Backend = backend;
    Backend.UseClient(this);
    Backend.NetworkStreamUpdated += OnUpdatedNetworkStream;
    StreamFeatureAdvertised += Backend.OnStreamFeatureAdvertised;

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
  }
  
  public async ValueTask DisposeAsync()
  {
    await DisconnectWithStreamCloseAsync();

    await _backgroundServiceTokenSource.CancelAsync();
    if (BackgroundServiceHandler != null) await BackgroundServiceHandler;
    
    Backend.NetworkStreamUpdated -= OnUpdatedNetworkStream;
    StreamFeatureAdvertised -= Backend.OnStreamFeatureAdvertised;

    if (Writer != null) await Writer.DisposeAsync();
    
    Backend.Dispose();
  }
  
  #region Connection Management

  public async Task<ConnectResult> ConnectAsync()
  {
    if (State != XmppState.Disconnected)
      return new ConnectResults.ClientAlreadyConnected();

    await LoadExtensions(XmppClientExtensionLoadAt.InstantActivateOnConnected);

    var backendConnectResult = await Backend.ConnectAsync(Address);
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
    
    await ActivateExtensions(XmppClientExtensionLoadAt.InstantActivateOnConnected);

    return new Unit();
  }

  public async Task<DisconnectResult> Disconnect()
  {
    if (State == XmppState.Disconnected)
      return new DisconnectResults.AlreadyDisconnected();

    await StopBackgroundService();

    State = XmppState.Disconnected;
    Backend.Disconnect();

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
    if (Writer is null)
      return new SendStanzaResults.WriterNullException();

    await WriteLock.WaitAsync();
    var serializer = new XmlSerializer(element.GetType());

    try
    {
      serializer.Serialize(Writer!, element);
    }
    catch (InvalidOperationException)
    {
      return new SendStanzaResults.SerializationFailure();
    }
    
    await Writer!.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }

  public async Task<SendStanzaResult> SendStanzaAsync(XElement element)
  {
    if (Writer is null)
      return new SendStanzaResults.WriterNullException();

    await WriteLock.WaitAsync();
    element.WriteTo(Writer);
    await Writer.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }
  
  public async Task<SendInfoQueryResult> SendInfoQueryAsync(InfoQuery query)
  {
    var id = Guid.CreateVersion7().ToString();
    query.Id ??= id;

    var tcs = new TaskCompletionSource<InfoQuery>(TaskCreationOptions.RunContinuationsAsynchronously);
    InfoQueries[id] = tcs;
    
    var stanzaResult = await SendStanzaAsync(query);
    if (!stanzaResult.IsT0)
      return stanzaResult.Match<SendInfoQueryResult>(
        _ => throw new UnreachableException(),
        _ => new SendInfoQueryResults.SerializationFailure(),
        _ => new SendInfoQueryResults.WriterNullException());
      
    var result = await tcs.Task;
    if (result.Type == InfoQueryType.Error)
      return new SendInfoQueryResults.InfoQueryError(result.ToString(), result.StanzaError!);
    return result;
  }

  public async Task<SendPresenceResult> SendPresenceAsync(Presence.Presence presence)
  {
    var id =  Guid.CreateVersion7().ToString();
    presence.Id ??= id;

    return (await SendStanzaAsync(presence)).Match<SendPresenceResult>(
      _ => new Unit(),
      _ => new SendPresenceResults.SerializationFailure(),
      _ => new SendPresenceResults.WriterNullException());
  }

  #endregion

  #region Element Registrations

  public RegisterUnexpectedStanzaResult RegisterUnexpectedStanza<T>(Func<object, object?, Task> func)
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.ElementName == null)
        return new RegisterUnexpectedStanzaResults.StanzaNameMissing();
      if (attr.Namespace == null)
        return new RegisterUnexpectedStanzaResults.StanzaNamespaceMissing();

      var key = $"{attr.Namespace}/{attr.ElementName}";

      if (UnexpectedStanzaSerializers.ContainsKey(key))
        return new RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered(key);

      UnexpectedStanzaSerializers.Add(key, (new XmlSerializer(typeof(T)), func));
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new RegisterUnexpectedStanzaResults.AmbiguousAttributeMatch();
    }
  }
  
  public UnregisterUnexpectedStanzaResult UnregisterUnexpectedStanza<T>()
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.ElementName == null)
        return new UnregisterUnexpectedStanzaResults.StanzaNameMissing();
      if (attr.Namespace == null)
        return new UnregisterUnexpectedStanzaResults.StanzaNamespaceMissing();

      var key = $"{attr.Namespace}/{attr.ElementName}";
      UnexpectedStanzaSerializers.Remove(key);
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new UnregisterUnexpectedStanzaResults.AmbiguousAttributeMatch();
    }
  }
  
  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new()
  {
    var mech = new T();
    mech.BindClient(this, Backend);
    SaslHandlers[mech.Priority] = mech;
  }
  
  #endregion

  #region Stream Management

  public async Task<OpenXmppStreamResult> OpenXmppStream()
  {
    if (Stream is null)
      return new OpenXmppStreamResults.StreamNullException();

    var to = Address.Host.TrimEnd(".").ToString();

    await WriteLock.WaitAsync();
    Stream!.Write("<?xml version='1.0'?>"u8.ToArray());
    Stream.Write(Encoding.UTF8.GetBytes(
      $"<stream:stream from='{Credentials.Jid}' to='{to}' version='1.0' xml:lang='en' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams'>\n"));
    await Stream.FlushAsync();
    WriteLock.Release();

    State = XmppState.Negotiating;
    return new Unit();
  }
  
  private async Task<CloseXmppStreamResult> CloseXmppStream()
  {
    if (Stream is null)
      return new CloseXmppStreamResults.StreamNullException();

    Console.WriteLine("Closing XMPP stream");

    await WriteLock.WaitAsync();
    await Stream!.WriteAsync("</stream:stream>"u8.ToArray());
    await Stream.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }

  #endregion

  #region Readers

  private async Task ReadStreamFeatures(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    await sub.ReadAsync(); // Skip Header

    while (await sub.ReadAsync())
      if (sub is { NodeType: XmlNodeType.Element, Depth: 1 })
      {
        Console.WriteLine($"F> {sub.Name}: {sub.NamespaceURI}");
        XmppClientRegistry.FeatureSerializers.TryGetValue(sub.NamespaceURI, out var featureSerializer);
        var feature = featureSerializer?.Deserialize(sub);
        if (feature != null)
          StreamFeatureAdvertised?.Invoke(this, new StreamFeatureRequestedEventArgs { Feature = feature });
      }

    ReadLock.Release();
  }
  
  private readonly XmlSerializer _infoQuerySerializer = new(typeof(InfoQuery));

  private void ReadInfoQuery(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    var infoQuery = (InfoQuery?)_infoQuerySerializer.Deserialize(sub);
    if (infoQuery == null)
      return;

    infoQuery.StanzaError?.Errors = ParseStanzaErrors(infoQuery.StanzaError.InternalErrors);
    infoQuery.DeserializeExtensions();
    
    ReadLock.Release();
    
    InfoQueries.TryGetValue(infoQuery.Id!, out var infoQueryTaskSource);
    if (infoQueryTaskSource is null)
      OnUnexpectedInfoQueryReceived?.Invoke(this,
        new OnUnexpectedInfoQueryReceivedEventArgs() { InfoQuery = infoQuery });
    else
      infoQueryTaskSource.TrySetResult(infoQuery);
  }
  
  private readonly XmlSerializer _messageSerializer = new(typeof(Message));

  private void ReadMessage(XmlReader reader)
  {
    ReadLock.Release();
    
    using var sub = reader.ReadSubtree();
    var message = (Message?)_messageSerializer.Deserialize(sub);
    
    if (message == null) return;
    
    if (message.StanzaError is not null)
    {
      var errors = ParseStanzaErrors(message.StanzaError.InternalErrors);
      message.StanzaError.Errors = errors;
    }

    OnMessageReceived?.Invoke(this, new OnMessageReceivedEventArgs { Message = message });
  }

  private readonly XmlSerializer _presenceSerializer = new(typeof(Presence.Presence));

  private void ReadPresence(XmlReader reader)
  {
    ReadLock.Release();
    
    using var sub = reader.ReadSubtree();
    var presence = (Presence.Presence?)_presenceSerializer.Deserialize(sub);

    if (presence == null) return;

    if (presence.StanzaError is not null)
    {
      var errors = ParseStanzaErrors(presence.StanzaError.InternalErrors);
      presence.StanzaError.Errors = errors;
    }
    
    OnPresenceReceived?.Invoke(this, new OnPresenceReceivedEventArgs { Presence = presence });
  }

  private void ReadUnexpectedStanza(XmlReader reader)
  {
    using var sub = reader.ReadSubtree();
    var found = UnexpectedStanzaSerializers.TryGetValue($"{reader.NamespaceURI}/{reader.Name}",
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
    await sub.ReadAsync(); // Skip Header
    await sub.ReadAsync(); // Read first error

    XmppClientRegistry.ErrorSerializers.TryGetValue($"{sub.NamespaceURI}/{sub.Name}", out var errorSerializer);
    return errorSerializer?.Deserialize(sub) as IClientError;
  }

  #endregion
  
  #region Background Service

  public void StartBackgroundService()
  {
    _backgroundServiceTokenSource = new CancellationTokenSource();
    BackgroundServiceHandler = Task.Run(BackgroundService);
  }

  public async Task StopBackgroundService()
  {
    await _backgroundServiceTokenSource.CancelAsync();
    if (BackgroundServiceHandler != null)
      await BackgroundServiceHandler;
    BackgroundServiceHandler = null;
  }
  
  private async Task BackgroundService()
  {
    using var reader = XmlReader.Create(Stream!, new XmlReaderSettings
    {
      Async = true, IgnoreProcessingInstructions = true, IgnoreWhitespace = true, IgnoreComments = true,
      CloseInput = false
    });

    while (!_backgroundServiceTokenSource.IsCancellationRequested)
    {
      // await ReadLock approval
      try
      {
        await ReadLock.WaitAsync(_backgroundServiceTokenSource.Token);
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
      
      switch (reader.Name)
      {
        case "stream:stream":
          ReadLock.Release();
          break;
        case "stream:features":
          await ReadStreamFeatures(reader);
          break;
        case "stream:error":
          InvokeClientError(await ReadSingleError(reader) ?? new GenericError());
          break;
        case "failure":
          if (reader.NamespaceURI == "urn:ietf:params:xml:ns:xmpp-sasl")
            InvokeClientError(await ReadSingleError(reader) ?? new GenericError());
          break;
        case "iq":
          ReadInfoQuery(reader);
          break;
        case "message":
          ReadMessage(reader);
          break;
        default:
          ReadUnexpectedStanza(reader);
          break;
      }
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
    return _enabledExtensions.And(extensions).HasAnySet();
  }

  public void EnableExtension<T>() where T : class, IXmppClientExtension<T>
  {
    var extension = T.Create(this);
    _extensions[T.ExtensionIdentifier] = extension;
    
    _enabledExtensions[T.ExtensionIdentifier] = true;
    _loadExtensionAt[(int)T.LoadAt][T.ExtensionIdentifier] = true;
  }

  public async Task DisableExtension<T>() where T : class, IXmppClientExtension<T>
  {
    var ext = GetExtension<T>();
    if (ext != null)
      await ext.DisposeAsync();
    
    _extensions.Remove(T.ExtensionIdentifier);
    _enabledExtensions[T.ExtensionIdentifier] = false;
    _loadExtensionAt[(int)T.LoadAt][T.ExtensionIdentifier] = false;
  }

  public T? GetExtension<T>() where T : class, IXmppClientExtension<T>
  {
    if (!_enabledExtensions[T.ExtensionIdentifier])
      return null;

    return _extensions[T.ExtensionIdentifier] as T;
  }

  private async Task LoadExtensions(XmppClientExtensionLoadAt load)
  {
    var enabled = _loadExtensionAt[(int)load];
    for (var i = 0; i < enabled.Length; i++)
    {
      if (!enabled[i]) continue;
      await _extensions[i].LoadAsync();
    }
  }
  
  private async Task ActivateExtensions(XmppClientExtensionLoadAt load)
  {
    var enabled = _loadExtensionAt[(int)load];
    for (var i = 0; i < enabled.Length; i++)
    {
      if (!enabled[i]) continue;
      await _extensions[i].ActivateAsync();
    }
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
    await OpenXmppStream();
  }
  
  private async void SaslHandler(object? sender, StreamFeatureRequestedEventArgs args)
  {
    try
    {
      if (args.Feature is not SaslFeature sasl)
        return;

      Console.WriteLine("Supported SASL mechanisms:");
      sasl.Mechanisms.ForEach(Console.WriteLine);

      var commonMechanisms = SaslHandlers
        .Where(mech => sasl.Mechanisms.Contains(mech.Value.Mechanism));
      
      var mechanism = commonMechanisms.First(); 
      Console.WriteLine($"Using P{mechanism.Key}: {mechanism.Value.Mechanism}");
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

      var resource = Credentials.Jid.Resource ?? Guid.NewGuid().ToString();
      Console.WriteLine($"Binding to resource {Credentials.Jid.Resource}");
      
      var query = new InfoQuery()
      {
        Type = InfoQueryType.Set,
        ResourceBind = new Bind()
        {
          Resource = resource,
        }
      };

      var result = await SendInfoQueryAsync(query);
      if (!result.IsT0)
      {
        InvokeClientError(new BindError(resource, (result.Value as IClientError)?.What()!));
        return;
      }
      
      var iq = result.AsT0;
      
      
      if (iq.Type == InfoQueryType.Error)
      {
        var errors = iq.StanzaError!.Errors.Select(e => e.What());
        InvokeClientError(new BindError(resource, string.Join(Environment.NewLine, errors)));
      }
      
      FullJid = Credentials.Jid with { Resource = iq.ResourceBind!.Resource };
      Console.WriteLine($"XMPP Client Connected to JID {FullJid}");

      State = XmppState.Connected;

      await LoadExtensions(XmppClientExtensionLoadAt.AndActivateOnSuccess);
      await ActivateExtensions(XmppClientExtensionLoadAt.AndActivateOnSuccess);
      
      EnableExtension<ImExtension>();
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

    Writer?.Dispose();
    Stream = stream;

    if (Stream is not null)
      Writer = XmlWriter.Create(Stream, new XmlWriterSettings
      {
        Async = true, CloseOutput = false, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Auto
      });
  }
  
  #endregion
}