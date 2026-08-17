namespace XMPP.Core.Errors;

public static class RegisterFeatureResults
{
  public record FeatureAlreadyRegistered(string key) : IClientError
  {
    public string What() => $"The provided feature namespace, {key}, is already registered";
  }
}