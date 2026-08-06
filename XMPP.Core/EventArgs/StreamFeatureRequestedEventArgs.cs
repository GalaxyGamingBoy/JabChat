namespace XMPP.Core.EventArgs;

public class StreamFeatureRequestedEventArgs : System.EventArgs
{
  public object Feature { get; init; }
}
