namespace XMPP.Core.Errors;

public static class OpenXmppStreamResults
{
  public record StreamNullException : IClientError
  {
    public string What() => "The XMPP client stream is null";
  }
}