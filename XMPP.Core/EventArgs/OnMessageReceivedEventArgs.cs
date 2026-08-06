using XMPP.Core.Messages;

namespace XMPP.Core.EventArgs;

public class OnMessageReceivedEventArgs : System.EventArgs
{
  public required Message Message { get; set; }
}