using System.Xml.Serialization;

namespace XMPP.Core.SaslMechanisms;

[XmlRoot("success", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public class SaslSuccess
{
  [XmlText]
  public required string Body;
}