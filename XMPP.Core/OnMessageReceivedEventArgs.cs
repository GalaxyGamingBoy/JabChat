namespace XMPP.Core;

public class OnMessageReceivedEventArgs : EventArgs
{
  public required Message Message { get; set; }
}