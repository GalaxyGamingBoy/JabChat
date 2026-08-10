using XMPP.Core.Messages;

namespace XMPP.Core.EventArgs;

public class OnPresenceReceivedEventArgs : System.EventArgs
{
  public required Presence.Presence Presence { get; set; }
}