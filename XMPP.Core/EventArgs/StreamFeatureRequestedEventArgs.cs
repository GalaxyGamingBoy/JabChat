namespace XMPP.Core.EventArgs;

public class StreamFeatureRequestedEventArgs : System.EventArgs
{
  public required object Feature { get; init; }
}
