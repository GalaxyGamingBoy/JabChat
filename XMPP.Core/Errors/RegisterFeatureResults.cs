namespace XMPP.Core.Errors;

public static class RegisterFeatureResults
{
  public record AmbiguousAttributeMatch : IClientError
  {
    public string What() => "Ambiguous attribute match for the type provided";
  }

  public record FeatureNamespaceAlreadyRegistered(string Namespace) : IClientError
  {
    public string What() => $"The provided feature namespace, {Namespace}, is already registered";
  }

  public record FeatureNamespaceMissing : IClientError
  {
    public string What() => "The provided feature does not have a namespace";
  }
}