using System.Diagnostics;
using System.Reflection;
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

using RegisterFeatureResult = OneOf<
  Unit,
  RegisterFeatureResults.AmbiguousAttributeMatch,
  RegisterFeatureResults.FeatureNamespaceAlreadyRegistered,
  RegisterFeatureResults.FeatureNamespaceMissing
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

using RegisterClientErrorResult = OneOf<
  Unit,
  RegisterClientErrorResults.AmbiguousAttributeMatch,
  RegisterClientErrorResults.XmlErrorNameMissing,
  RegisterClientErrorResults.XmlErrorNamespaceMissing,
  RegisterClientErrorResults.ErrorAlreadyRegistered
>;

public class XmppClient : IXmppClient, IAsyncDisposable
{
  #region Internal Fields

  private Stream? _stream;
  
  private XmlWriter? _writer;
  
  private Dictionary<string, XmlSerializer> FeatureSerializers { get; } = new();
  
  private Dictionary<string, XmlSerializer> ErrorSerializers { get; } = new();
  
  private Dictionary<string, (XmlSerializer, Func<object, object?, Task>)> UnexpectedStanzaSerializers { get; } = new();
  
  private SortedList<int, ISaslMechanism> SaslHandlers { get; } = new();
  
  private Dictionary<string, TaskCompletionSource<InfoQuery>> InfoQueries { get; } = new();

  private CancellationTokenSource _cts = new();
  
  private Task? BackgroundServiceHandler { get; set; }
  
  private XmppJid? FullJid { get; set; }

  private IXmppClientBackend Backend { get; init; }
  
  #endregion

  #region Public Fields
  
  // ReSharper disable once MemberCanBePrivate.Global - Useful for applications
  public XmppState State { get; set; } = XmppState.Disconnected;

  public required XmppAddress Address { get; init; }
  
  public required XmppCredentials Credentials { get; init; }

  #endregion

  public XmppClient(IXmppClientBackend backend)
  {
    // Stream Features
    RegisterFeature<StartTlsFeature>();
    RegisterFeature<SaslFeature>();
    RegisterFeature<BindFeature>();

    // Errors - StreamErrors
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

    // Errors - SaslErrors
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

    // Errors - StanzaErrors
    RegisterClientError<StanzaErrors.BadRequest>();
    RegisterClientError<StanzaErrors.Conflict>();
    RegisterClientError<StanzaErrors.FeatureNotImplemented>();
    RegisterClientError<StanzaErrors.Forbidden>();
    RegisterClientError<StanzaErrors.Gone>();
    RegisterClientError<StanzaErrors.InternalServerError>();
    RegisterClientError<StanzaErrors.ItemNotFound>();
    RegisterClientError<StanzaErrors.JidMalformed>();
    RegisterClientError<StanzaErrors.NotAcceptable>();
    RegisterClientError<StanzaErrors.NotAllowed>();
    RegisterClientError<StanzaErrors.NotAuthorized>();
    RegisterClientError<StanzaErrors.PolicyViolation>();
    RegisterClientError<StanzaErrors.RecipientUnavailable>();
    RegisterClientError<StanzaErrors.Redirect>();
    RegisterClientError<StanzaErrors.RegistrationRequired>();
    RegisterClientError<StanzaErrors.RemoteServerNotFound>();
    RegisterClientError<StanzaErrors.RemoteServerTimeout>();
    RegisterClientError<StanzaErrors.ResourceConstraint>();
    RegisterClientError<StanzaErrors.ServiceUnavailable>();
    RegisterClientError<StanzaErrors.SubscriptionRequired>();
    RegisterClientError<StanzaErrors.UndefinedCondition>();
    RegisterClientError<StanzaErrors.UnexpectedRequest>();

    // Backend Configuration
    Backend = backend;
    Backend.UseClient(this);
    Backend.NetworkStreamUpdated += OnUpdatedNetworkStream;
    StreamFeatureAdvertised += Backend.OnStreamFeatureRequested;

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
    await Disconnect();

    await _cts.CancelAsync();
    if (BackgroundServiceHandler != null) await BackgroundServiceHandler;

    if (_writer != null) await _writer.DisposeAsync();

    Backend.Dispose();
  }
  
  #region Connection Management

  public async Task<ConnectResult> ConnectAsync()
  {
    if (State != XmppState.Disconnected)
      return new ConnectResults.ClientAlreadyConnected();

    var backendConnectResult = await Backend.ConnectAsync(Address);
    if (!backendConnectResult.IsT0)
      return backendConnectResult.Match<ConnectResult>(
        _ => new Unit(),
        _ => new ConnectResults.AddressPortInvalid(), 
        _ => new ConnectResults.ClientAlreadyConnected(),
        _ => new ConnectResults.ConnectionFailure());
    
    State = XmppState.SocketConnected;
    
    var streamResult = await OpenXmppStream();
    if (!streamResult.IsT0)
      return streamResult.Match<ConnectResult>(
        _ => new Unit(),
        _ => new ConnectResults.ClientAlreadyConnected());
    
    StartBackgroundService();

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

    var tcs = new TaskCompletionSource<InfoQuery>();
    InfoQueries[id] = tcs;
    
    var stanzaResult = await SendStanzaAsync(query);
    if (!stanzaResult.IsT0)
      return stanzaResult.Match<SendInfoQueryResult>(
        _ => throw new UnreachableException(),
        _ => new SendInfoQueryResults.SerializationFailure(),
        _ => new SendInfoQueryResults.WriterNullException());
      
    var result = await tcs.Task;
    if (result.Type == InfoQueryType.Error)
      return new SendInfoQueryResults.InfoQueryError(result.ToString());
    return result;
  }
  
  #endregion

  #region Element Registrations

  public RegisterFeatureResult RegisterFeature<T>()
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.Namespace == null)
        return new RegisterFeatureResults.FeatureNamespaceMissing();

      if (FeatureSerializers.ContainsKey(attr.Namespace))
        return new RegisterFeatureResults.FeatureNamespaceAlreadyRegistered(attr.Namespace);

      FeatureSerializers.Add(attr.Namespace, new XmlSerializer(typeof(T)));
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new RegisterFeatureResults.AmbiguousAttributeMatch();
    }
  }

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
  
  public RegisterClientErrorResult RegisterClientError<T>() where T : IClientError
  {
    try
    {
      var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
        typeof(T), typeof(XmlRootAttribute));

      if (attr?.ElementName == null)
        return new RegisterClientErrorResults.XmlErrorNameMissing();
      if (attr.Namespace == null)
        return new RegisterClientErrorResults.XmlErrorNamespaceMissing();

      var key = $"{attr.Namespace}/{attr.ElementName}";

      if (ErrorSerializers.ContainsKey(key))
        return new RegisterClientErrorResults.ErrorAlreadyRegistered(key);

      ErrorSerializers.Add(key, new XmlSerializer(typeof(T)));
      return new Unit();
    }
    catch (AmbiguousMatchException)
    {
      return new RegisterClientErrorResults.AmbiguousAttributeMatch();
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
    if (_stream is null)
      return new OpenXmppStreamResults.StreamNullException();

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

    Console.WriteLine("Closing XMPP stream");

    await WriteLock.WaitAsync();
    await _stream!.WriteAsync("</stream:stream>"u8.ToArray());
    await _stream.FlushAsync();
    WriteLock.Release();

    return new Unit();
  }

  #endregion

  #region Background Service

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
    var messageSerializer = new XmlSerializer(typeof(Message));

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
          ClientErrorRaised?.Invoke(this, new ClientErrorRaisedEventArgs() { Error = (IClientError)error });

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
          ClientErrorRaised?.Invoke(this, new ClientErrorRaisedEventArgs() { Error = (IClientError)error });

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
              StreamFeatureAdvertised?.Invoke(this, new StreamFeatureRequestedEventArgs { Feature = feature });
          }

        ReadLock.Release();
        continue;
      }

      if (reader.Name == "iq")
      {
        using var sub = reader.ReadSubtree();
        var infoQuery = (InfoQuery?)infoQuerySerializer.Deserialize(sub);
        if (infoQuery != null)
        {
          if (infoQuery.StanzaError is not null)
          {
            var errors = ParseStanzaErrors(infoQuery.StanzaError.InternalErrors);
            infoQuery.StanzaError.Errors = errors;
          }

          InfoQueries.TryGetValue(infoQuery.Id!, out var infoQueryTaskSource);
          infoQueryTaskSource?.TrySetResult(infoQuery);
        }

        ReadLock.Release();
        continue;
      }

      if (reader.Name == "message")
      {
        using var sub = reader.ReadSubtree();
        var message = (Message?)messageSerializer.Deserialize(sub);
        if (message != null)
        {
          if (message.StanzaError is not null)
          {
            var errors = ParseStanzaErrors(message.StanzaError.InternalErrors);
            message.StanzaError.Errors = errors;
          }

          OnMessageReceived?.Invoke(this, new OnMessageReceivedEventArgs { Message = message });
        }
      }

      {
        using var sub = reader.ReadSubtree();
        var found = UnexpectedStanzaSerializers.TryGetValue($"{reader.NamespaceURI}/{reader.Name}",
          out var stanzaSerializer);

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
        ErrorSerializers.TryGetValue($"{element.NamespaceURI}/{element.Name}", out var errorSerializer);
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

      foreach (var mechanism in SaslHandlers
                 .Where(mechanism
                   => sasl.Mechanisms.Contains(mechanism.Value.Mechanism)))
      {
        Console.WriteLine($"Using P{mechanism.Key}: {mechanism.Value.Mechanism}");
        await mechanism.Value.Use(Credentials);
        break;
      }
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

      FullJid = Credentials.Jid with { Resource = resource };

      Console.WriteLine($"XMPP Client Connected to JID {FullJid}");

      State = XmppState.Connected;
    }
    catch (Exception)
    {
      // ignored
    }
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
  
  #endregion
}