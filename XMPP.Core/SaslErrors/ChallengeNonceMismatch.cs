using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

public record ChallengeNonceMismatch(string Mechanism) : IClientError, IDefaultStanzaKey<ChallengeNonceMismatch>
{
  public string What()
  {
    return $"There was a mismatch on the nonce returned by the server while responding to a {Mechanism} challenge";
  }
};