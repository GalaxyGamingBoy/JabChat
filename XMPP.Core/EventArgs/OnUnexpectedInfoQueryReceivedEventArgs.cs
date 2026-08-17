using XMPP.Core.InfoQueries;

namespace XMPP.Core.EventArgs;

public class OnUnexpectedInfoQueryReceivedEventArgs : System.EventArgs
{
  public required InfoQuery InfoQuery { get; set; }
}