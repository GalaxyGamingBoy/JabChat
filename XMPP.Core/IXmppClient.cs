using System.Xml.Linq;
using OneOf;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.InfoQueries;
using XMPP.Core.SaslMechanisms;

namespace XMPP.Core;

public interface IXmppClient
{
  #region Connection Management

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
  /// Disconnects from the XMPP server without the stream closing
  /// </summary>
  public Task<OneOf<
    Unit,
    DisconnectResults.StreamNullException,
    DisconnectResults.AlreadyDisconnected
  >> Disconnect();

  /// <summary>
  /// Disconnect cleanly from the XMPP server
  /// </summary>
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

  #endregion

  #region Messsage Management

  /// <summary>
  /// Sends a stanza element over the stream to the server
  /// </summary>
  /// <param name="element">stanza element to send</param>
  public Task<OneOf<
    Unit,
    SendStanzaResults.SerializationFailure,
    SendStanzaResults.WriterNullException
  >> SendStanzaAsync(object element);
  
  /// <summary>
  /// Sends a stanza element over the stream to the server
  /// </summary>
  /// <param name="element">stanza element to send</param>
  public Task<OneOf<
    Unit,
    SendStanzaResults.SerializationFailure,
    SendStanzaResults.WriterNullException
  >> SendStanzaAsync(XElement element);
  
  /// <summary>
  /// Sends an info-query to the server
  /// </summary>
  /// <param name="query">InfoQuery element</param>
  public Task<OneOf<
    InfoQuery,
    SendInfoQueryResults.InfoQueryError,
    SendInfoQueryResults.SerializationFailure,
    SendInfoQueryResults.WriterNullException
  >> SendInfoQueryAsync(InfoQuery query);
  
  /// <summary>
  /// Manages the ReadLock of the backend service, usually there is no need to touch (exceptions exist)
  ///
  /// The only time you are supposed to release the ReadLock is when:
  ///   + You receive an unexpected stanza, the ReadLock is transferred to you
  /// </summary>
  public SemaphoreSlim ReadLock { get; }

  #endregion

  #region Element Registrations

  /// <summary>
  /// Registers a XMPP stream feature for deserialization
  /// </summary>
  /// <typeparam name="T">Feature Element</typeparam>
  public OneOf<
    Unit,
    RegisterFeatureResults.AmbiguousAttributeMatch,
    RegisterFeatureResults.FeatureNamespaceAlreadyRegistered,
    RegisterFeatureResults.FeatureNamespaceMissing
  > RegisterFeature<T>();

  /// <summary>
  /// Registers an unexpected stanza to act on
  /// </summary>
  /// <param name="func">Function to call</param>
  /// <typeparam name="T">Stanza Element</typeparam>
  public OneOf<
    Unit,
    RegisterUnexpectedStanzaResults.AmbiguousAttributeMatch,
    RegisterUnexpectedStanzaResults.StanzaNameMissing,
    RegisterUnexpectedStanzaResults.StanzaNamespaceMissing,
    RegisterUnexpectedStanzaResults.UnexpectedStanzaAlreadyRegistered
  > RegisterUnexpectedStanza<T>(Func<object, object?, Task> func);
  
  /// <summary>
  /// Unregisters an unexpected stanza
  /// </summary>
  /// <typeparam name="T">Stanza Element</typeparam>
  public OneOf<
    Unit,
    UnregisterUnexpectedStanzaResults.AmbiguousAttributeMatch,
    UnregisterUnexpectedStanzaResults.StanzaNameMissing,
    UnregisterUnexpectedStanzaResults.StanzaNamespaceMissing
  > UnregisterUnexpectedStanza<T>();

  /// <summary>
  /// Registers a client error
  /// </summary>
  /// <typeparam name="T">Client Error Element</typeparam>
  public OneOf<
    Unit,
    RegisterClientErrorResults.AmbiguousAttributeMatch,
    RegisterClientErrorResults.XmlErrorNameMissing,
    RegisterClientErrorResults.XmlErrorNamespaceMissing,
    RegisterClientErrorResults.ErrorAlreadyRegistered
  > RegisterClientError<T>() where T : IClientError;

  /// <summary>
  /// Registers a SaslMechanism
  /// </summary>
  /// <typeparam name="T">Sasl Mechanism</typeparam>
  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new();

  #endregion

  #region Stream Management

  /// <summary>
  /// Opens an XMPP stream
  /// </summary>
  public Task<OneOf<
    Unit,
    OpenXmppStreamResults.StreamNullException
  >> OpenXmppStream(); 

  #endregion

  #region Background Service

  /// <summary>
  /// Starts a background service
  /// </summary>
  public void StartBackgroundService();
  
  /// <summary>
  /// Stops a background service
  /// </summary>
  public Task StopBackgroundService();

  #endregion
  
  #region Callbacks
  
  /// <summary>
  /// Raised when a stream feature is advertised by the server
  /// </summary>
  event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureAdvertised;
  
  /// <summary>
  /// Raised when a message is received by the client
  /// </summary>
  event EventHandler<OnMessageReceivedEventArgs>? OnMessageReceived;
  
  /// <summary>
  /// Raised by the Sasl handler when Sasl negotiations are complete
  /// </summary>
  public Task SaslCompleted();

  #endregion

  #region Error Handling

  /// <summary>
  /// Raised when a client error raised
  /// </summary>
  event EventHandler<ClientErrorRaisedEventArgs>? ClientErrorRaised;
  
  /// <summary>
  /// Invokes a ClientErrorRaised event
  /// </summary>
  /// <param name="error">Error to raise</param>
  public void InvokeClientError(IClientError error);

  #endregion
}