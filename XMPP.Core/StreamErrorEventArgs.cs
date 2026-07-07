using System.Net.Sockets;
using XMPP.Core.StreamErrors;

namespace XMPP.Core;

public class StreamErrorEventArgs : EventArgs
{
  public IClientError Error { get; init; }
}
