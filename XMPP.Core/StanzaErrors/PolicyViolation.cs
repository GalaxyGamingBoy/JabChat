using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.12
/// </summary>
[XmlRoot("policy-violation", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record PolicyViolation : IClientError
{
  public string What()
  {
    return
      "The entity has violated some local service policy";
  }
};