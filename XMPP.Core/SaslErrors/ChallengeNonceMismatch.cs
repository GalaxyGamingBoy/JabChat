using XMPP.Core.ClientErrors;

namespace XMPP.Core.SaslErrors;

public record ChallengeNonceMismatch(string Mechanism) : IClientError
{
  public string What()
  {
    return $"There was a mismatch on the nonce returned by the server while responding to a {Mechanism} challenge";
  }
};