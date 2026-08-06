using XMPP.Core.Errors;

namespace XMPP.Core.EventArgs;

public class ClientErrorRaisedEventArgs : System.EventArgs
{
  public IClientError Error { get; init; }
}
