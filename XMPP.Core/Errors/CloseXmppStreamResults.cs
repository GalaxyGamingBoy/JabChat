using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class CloseXmppStreamResults
{
  public record StreamNullException : IClientError
  {
    public string What() => "The XMPP client stream is null";
  }
}