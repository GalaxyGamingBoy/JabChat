using System.Net.Sockets;
using XMPP.Core.StreamErrors;

namespace XMPP.Core;

public class UnexpectedStanzaReceivedEventArgs : EventArgs
{
  public object Element { get; init; }
}
