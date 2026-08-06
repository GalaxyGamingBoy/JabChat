using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

public record ServerSignatureMismatch(string Mechanism) : IClientError
{
  public string What()
  {
    return $"There was a mismatch on the signature returned by the server while responding to a {Mechanism} challenge compared to the client calculated one";
  }
};