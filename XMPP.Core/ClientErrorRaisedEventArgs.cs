using System.Net.Sockets;
using XMPP.Core.ClientErrors;
using XMPP.Core.StreamErrors;

namespace XMPP.Core;

public class ClientErrorRaisedEventArgs : EventArgs
{
  public IClientError Error { get; init; }
}
