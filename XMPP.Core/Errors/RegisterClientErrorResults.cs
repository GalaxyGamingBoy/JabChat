namespace XMPP.Core.Errors;

public static class RegisterClientErrorResults
{
  public record AlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Client error with key {Key} already registered";
  }
}