using System.Net.Sockets;
using XMPP.Core.StreamErrors;

namespace XMPP.Core;

public class StreamErrorEventArgs : EventArgs
{
  public IStreamError Error { get; init; }
}
