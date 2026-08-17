namespace XMPP.Core.Errors;

public static class RegisterFeatureResults
{
  public record FeatureAlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"The provided feature namespace, {Key}, is already registered";
  }
}