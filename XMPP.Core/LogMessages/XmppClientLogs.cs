using Microsoft.Extensions.Logging;

namespace XMPP.Core.LogMessages;

public static partial class XmppClientLogs
{
  [LoggerMessage(LogLevel.Information, Message = "Configuring XMPP client")]
  public static partial void ConfiguringClient(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "XMPP client configuration completed")]
  public static partial void CompletedClientConfiguration(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Registered unexpected stanza handler for {Key}")]
  public static partial void RegisteredUnexpectedStanza(ILogger logger, string key);
  
  [LoggerMessage(LogLevel.Error, Message = "An unexpected stanza was already registered for {Key}")]
  public static partial void ConflictingUnexpectedStanza(ILogger logger, string key);
  
  [LoggerMessage(LogLevel.Information, Message = "Unregistered unexpected stanza handler for {Key}")]
  public static partial void UnregisteredUnexpectedStanza(ILogger logger, string key);
  
  [LoggerMessage(LogLevel.Information, Message = "Registered SASL mechanism {Mechanism} with priority {Priority}")]
  public static partial void RegisteredSaslMechanism(ILogger logger, string mechanism, int priority);

  [LoggerMessage(LogLevel.Information, Message = "Binding the XMPP client with the backend")]
  public static partial void BindingBackend(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Enabling extension {Extension} with identifier {Id}")]
  public static partial void EnablingExtension(ILogger logger, string? extension, int id);
  
  [LoggerMessage(LogLevel.Information, Message = "Disabling extension {Extension} with identifier {Id}")]
  public static partial void DisablingExtension(ILogger logger, string? extension, int id);
  
  [LoggerMessage(LogLevel.Information, Message = "Disposing XMPP client")]
  public static partial void DisposingClient(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Establishing a XMPP connection to {Host}")]
  public static partial void Connecting(ILogger logger, string host);
  
  [LoggerMessage(LogLevel.Information, Message = "Disconnecting client from {Host}")]
  public static partial void Disconnecting(ILogger logger, string host);
  
  [LoggerMessage(LogLevel.Information, Message = "Attempting to reconnect to {Host}")]
  public static partial void Reconnecting(ILogger logger, string host);
  
  [LoggerMessage(LogLevel.Debug, Message = "Opening a XMPP stream")]
  public static partial void OpeningXmppStream(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Closing a XMPP stream")]
  public static partial void ClosingXmppStream(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Starting the Background Service...")]
  public static partial void StartBackgroundService(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Stopping the Background Service...")]
  public static partial void StopBackgroundService(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "XML reader waiting for ReadLock")]
  public static partial void ReaderWaitingReadLock(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "XML reader pass has begun")]
  public static partial void ReaderPassBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "XML reader pass has ended")]
  public static partial void ReaderPassEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read InfoQuery event begun")]
  public static partial void ReadInfoQueryBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read InfoQuery event ended")]
  public static partial void ReadInfoQueryEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read Message event begun")]
  public static partial void ReadMessageBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read Message event ended")]
  public static partial void ReadMessageEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read Presence event begun")]
  public static partial void ReadPresenceBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read Presence event ended")]
  public static partial void ReadPresenceEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read UnexpectedStanza event begun")]
  public static partial void ReadUnexpectedStanzaBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read UnexpectedStanza event ended")]
  public static partial void ReadUnexpectedStanzaEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read StreamFeature event ended")]
  public static partial void ReadStreamFeatureBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Read StreamFeature event ended")]
  public static partial void ReadStreamFeatureEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Critical, Message = "Encountered a stream error")]
  public static partial void EncounteredStreamError(ILogger logger);
  
  [LoggerMessage(LogLevel.Critical, Message = "Encountered a sasl error")]
  public static partial void EncounteredSaslError(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Encountered a stream header")]
  public static partial void EncounteredStreamHeader(ILogger logger);
  
  [LoggerMessage(LogLevel.Warning, Message = "A non xml element was read, skipping")]
  public static partial void NonXmlElementReadSkipping(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "XMPP stream feature read, {Feature}")]
  public static partial void ReadStreamFeature(ILogger logger, string feature);
  
  [LoggerMessage(LogLevel.Debug, Message = "XML element read, NS: {Ns}, Name: {Name}")]
  public static partial void ReadXmlElement(ILogger logger, string ns, string name);

  [LoggerMessage(LogLevel.Information, Message = "SASL authentication begun")]
  public static partial void SaslAuthenticationBegun(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "SASL authentication ended")]
  public static partial void SaslAuthenticationEnded(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "Resource binding begun, attempted to bind to {Resource}")]
  public static partial void BindResourceBegun(ILogger logger, string resource);
  
  [LoggerMessage(LogLevel.Information, Message = "Resource binding ended, binded to {Resource}")]
  public static partial void BindResourceEnded(ILogger logger, string resource);
  
  [LoggerMessage(LogLevel.Information, Message = "XMPP client connected successfully with final jid {Jid}")]
  public static partial void Connected(ILogger logger, XmppJid jid);

  [LoggerMessage(LogLevel.Information, Message = "SASL server supports mechanisms: {Mechanisms}")]
  public static partial void SaslServerSupports(ILogger logger, string mechanisms);
  
  [LoggerMessage(LogLevel.Information, Message = "Using SASL mechanism {Mechanism} with priority {Priority}")]
  public static partial void UsingSaslMechanism(ILogger logger, string mechanism, int priority);

  [LoggerMessage(LogLevel.Debug, Message = "XMPP client network stream updated")]
  public static partial void NetworkStreamUpdated(ILogger logger);
  
  [LoggerMessage(LogLevel.Debug, Message = "Sending XMPP message with id: {Id}")]
  public static partial void SendingMessage(ILogger logger, string id);
  
  [LoggerMessage(LogLevel.Debug, Message = "Sending XMPP presence with id: {Id}")]
  public static partial void SendingPresence(ILogger logger, string id);
  
  [LoggerMessage(LogLevel.Debug, Message = "Sending XMPP info-query with id: {Id}")]
  public static partial void SendingInfoQuery(ILogger logger, string id);
  
  [LoggerMessage(LogLevel.Debug, Message = "Received InfoQuery result with id: {Id}")]
  public static partial void ReceivedInfoQueryResult(ILogger logger, string id);
}