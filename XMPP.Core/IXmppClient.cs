using System.Xml;
using System.Xml.Linq;
using FluentResults;
using OneOf;
using XMPP.Core.ClientErrors;
using XMPP.Core.Errors;
using XMPP.Core.InfoQueries;
using XMPP.Core.SaslMechanisms;
using XMPP.Core.StreamErrors;
using Result = FluentResults.Result;

namespace XMPP.Core;

public interface IXmppClient
{
  /// <summary>
  /// Connects asynchronously to the XMPP Server
  /// </summary>
  public Task<OneOf<
    Unit,
    ConnectResults.AddressPortInvalid,
    ConnectResults.ClientAlreadyConnected,
    ConnectResults.ConnectionFailure
  >> ConnectAsync();

  /// <summary>
  /// Disconnects from the XMPP Server
  /// </summary>
  public Task<OneOf<
    Unit,
    DisconnectResults.StreamNullException,
    DisconnectResults.AlreadyDisconnected
  >> Disconnect();

  public Task<OneOf<
    Unit,
    DisconnectResults.StreamNullException,
    DisconnectResults.AlreadyDisconnected
  >> DisconnectWithStreamCloseAsync();
  
  /// <summary>
  /// Reconnects asynchronously to the XMPP Server
  /// </summary>
  public Task<OneOf<
    Unit,
    ReconnectResults.AddressPortInvalid,
    ReconnectResults.ClientAlreadyConnected,
    ReconnectResults.ReconnectionFailure
  >> ReconnectAsync();

  public Task<OneOf<
    Unit,
    SendStanzaResults.SerializationFailure,
    SendStanzaResults.WriterNullException
  >> SendStanzaAsync(object element);
  
  public Task<OneOf<
    Unit,
    SendStanzaResults.SerializationFailure,
    SendStanzaResults.WriterNullException
  >> SendStanzaAsync(XElement element);
  
  public Task<OneOf<
    InfoQuery,
    SendInfoQueryResults.InfoQueryError,
    SendInfoQueryResults.SerializationFailure,
    SendInfoQueryResults.WriterNullException
  >> SendInfoQueryAsync(InfoQuery query);

  /// <summary>
  /// Registers a XMPP stream feature for deserialization
  /// </summary>
  /// <typeparam name="T">Feature Object</typeparam>
  public OneOf<
    Unit,
    RegisterFeatureResults.AmbiguousAttributeMatch,
    RegisterFeatureResults.FeatureNamespaceAlreadyRegistered,
    RegisterFeatureResults.FeatureNamespaceMissing
  > RegisterFeature<T>();

  public OneOf<
    Unit,
    RegisterUnexpectedStanzaResults.AmbiguousAttributeMatch,
    RegisterUnexpectedStanzaResults.StanzaNameMissing,
    RegisterUnexpectedStanzaResults.StanzaNamespaceMissing,
    RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered
  > RegisterUnexpectedStanza<T>(Func<object, object?, Task> func);


  public OneOf<
    Unit,
    RegisterClientErrorResults.AmbiguousAttributeMatch,
    RegisterClientErrorResults.XmlErrorNameMissing,
    RegisterClientErrorResults.XmlErrorNamespaceMissing,
    RegisterClientErrorResults.ErrorAlreadyRegistered
  > RegisterClientError<T>() where T : IClientError;

  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new();

  public Task<OneOf<Unit, OpenXmppStreamResults.StreamNullException>> OpenXmppStream();
  
  event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureRequested;
  event EventHandler<ClientErrorRaisedEventArgs>? ClientErrorRaised;
  event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;
  
  public Task SaslCompleted();

  public void StartBackgroundService();
  public Task StopBackgroundService();

  public void InvokeClientError(IClientError error);
  
  /// <summary>
  /// Manages the ReadLock of the backend service, usually there is no need to touch (exceptions exist)
  ///
  /// The only time you are supposed to release the ReadLock is when:
  ///   + You receive an unexpected stanza, the readlock is transferred to you
  /// </summary>
  public SemaphoreSlim ReadLock { get; }
}