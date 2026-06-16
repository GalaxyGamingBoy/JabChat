using System.Net.Sockets;

namespace XMPP.Core.Backend;

public class NetworkStreamUpdatedEventArgs : EventArgs
{
  public Stream? Stream { get; set; }
}