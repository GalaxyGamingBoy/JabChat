namespace XMPP.Core.Errors;

public static class RegisterUnexpectedStanzaResults
{
  public record UnexpectedStanzaAlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Unexpected Stanza with key {Key} already registered";
  }
}