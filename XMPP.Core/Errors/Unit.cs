namespace XMPP.Core.Errors;

public record Unit : IClientError
{
  public string What() => "No Error";
};