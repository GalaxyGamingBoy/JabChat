using XMPP.Core.InfoQueries;
using XMPP.Core.Messages;

namespace XMPP.Core.EventArgs;

public class OnUnexpectedInfoQueryReceivedEventArgs : System.EventArgs
{
  public required InfoQuery InfoQuery { get; set; }
}