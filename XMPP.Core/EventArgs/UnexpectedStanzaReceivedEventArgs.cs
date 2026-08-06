namespace XMPP.Core.EventArgs;

public class UnexpectedStanzaReceivedEventArgs : System.EventArgs
{
  public object Element { get; init; }
}
