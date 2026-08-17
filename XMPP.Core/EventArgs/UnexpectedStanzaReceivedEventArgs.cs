namespace XMPP.Core.EventArgs;

public class UnexpectedStanzaReceivedEventArgs : System.EventArgs
{
  public required object Element { get; init; }
}
