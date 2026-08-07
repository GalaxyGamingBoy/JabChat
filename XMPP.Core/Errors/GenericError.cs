namespace XMPP.Core.Errors;

public record GenericError : IClientError
{
  public string What()
  {
    return "A generic error occured";
  }
};