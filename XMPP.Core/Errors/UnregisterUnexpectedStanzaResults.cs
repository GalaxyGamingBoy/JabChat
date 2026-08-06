namespace XMPP.Core.Errors;

public static class UnregisterUnexpectedStanzaResults
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
}