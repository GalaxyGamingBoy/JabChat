using System.Net.Sockets;

namespace XMPP.Core;

public class StreamFeatureRequestedEventArgs : EventArgs
{
  public object Feature { get; init; }
}
