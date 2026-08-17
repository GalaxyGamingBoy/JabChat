namespace XMPP.Core.Errors;

public static class RegisterInfoQueryResults
{
  public record AlreadyRegistered(string Key) : IClientError
  {
    public string What() => $"Info Query with key {Key} already registered";
  }
}