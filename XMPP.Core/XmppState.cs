namespace XMPP.Core;

/// <summary>
/// XMPP Client State
/// </summary>
public enum XmppState
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
