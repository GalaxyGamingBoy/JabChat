using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.14
/// </summary>
[XmlRoot("policy-violation", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record PolicyViolation : IClientError
{
  public string What()
  {
    return "The entity has violated some local service policy";
  }
}