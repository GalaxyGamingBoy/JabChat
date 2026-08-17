using XMPP.Core.Errors;

namespace XMPP.Core.EventArgs;

public class ClientErrorRaisedEventArgs : System.EventArgs
{
  public required IClientError Error { get; init; }
}
