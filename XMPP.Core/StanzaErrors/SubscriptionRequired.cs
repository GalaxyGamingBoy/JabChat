using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.20
/// </summary>
[XmlRoot("subscription-required", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record SubscriptionRequired : IClientError, IDefaultStanzaKey<SubscriptionRequired>
{
  public string What()
  {
    return
      "The requesting entity is not authorized to access the requested service because a prior subscription is necessary";
  }
};