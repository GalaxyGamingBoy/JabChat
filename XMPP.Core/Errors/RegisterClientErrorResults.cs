using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class RegisterClientErrorResults
{
  public record AmbiguousAttributeMatch : IClientError
  {
    public string What() => "Ambiguous attribute match for the type provided";
  }

  public record XmlErrorNameMissing : IClientError
  {
    public string What() => "Unexpected Stanza name missing";
  }

  public record XmlErrorNamespaceMissing : IClientError
  {
    public string What() => "Unexpected Stanza namespace missing";
  }

  public record ErrorAlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Unexpected Stanza with key {Key} already registered";
  }
}