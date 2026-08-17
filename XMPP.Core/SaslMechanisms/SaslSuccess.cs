using System.Xml.Serialization;

namespace XMPP.Core.SaslMechanisms;

[XmlRoot("success", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record SaslSuccess : IDefaultStanzaKey<SaslSuccess>
{
  [XmlText]
  public required string Body;
}