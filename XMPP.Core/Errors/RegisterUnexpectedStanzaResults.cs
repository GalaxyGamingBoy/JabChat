namespace XMPP.Core.Errors;

public static class RegisterUnexpectedStanzaResults
{
  public record AmbiguousAttributeMatch : IClientError
  {
    public string What() => "Ambiguous attribute match for the type provided";
  }

  public record StanzaNameMissing : IClientError
  {
    public string What() => "Unexpected Stanza name missing";
  }

  public record StanzaNamespaceMissing : IClientError
  {
    public string What() => "Unexpected Stanza namespace missing";
  }

  public record UnexpectedStanzaAlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Unexpected Stanza with key {Key} already registered";
  }
}