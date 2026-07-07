using System.Xml.Serialization;

namespace XMPP.Core.SaslMechanisms;

[XmlRoot("challenge", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record ScramChallenge
{
  [XmlText]
  public required string Body;
}