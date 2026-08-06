namespace XMPP.Core.EventArgs;

public class NetworkStreamUpdatedEventArgs : System.EventArgs
{
  public Stream? Stream { get; set; }
}