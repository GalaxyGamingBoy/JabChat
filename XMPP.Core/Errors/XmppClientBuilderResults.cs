using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class XmppClientBuilderResults
{
  public record HostResolutionFailure : IClientError
  {
    public string What() => "A valid address or host was not specified that could resolve to a working IP";
  }

  public record UnspecifiedUsername : IClientError
  {
    public string What() => "Username was not specified";
  }

  public record UnspecifiedPassword : IClientError
  {
    public string What() => "Password was not specified";
  }
}