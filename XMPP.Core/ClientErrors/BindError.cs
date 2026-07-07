namespace XMPP.Core.ClientErrors;

public record BindError(string Resource, string Error) : IClientError
{
  public string What()
  {
    return $"Failed to bind to resource {Resource}. {Error}";
  }
}