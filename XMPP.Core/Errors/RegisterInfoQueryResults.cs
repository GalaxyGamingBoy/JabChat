namespace XMPP.Core.Errors;

public static class RegisterInfoQueryResults
{
  public record AmbiguousAttributeMatch : IClientError
  {
    public string What() => "Ambiguous attribute match for the type provided";
  }

  public record XmlErrorNameMissing : IClientError
  {
    public string What() => "Info Query name missing";
  }

  public record XmlErrorNamespaceMissing : IClientError
  {
    public string What() => "Info Query namespace missing";
  }

  public record ErrorAlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Info Query with key {Key} already registered";
  }
}