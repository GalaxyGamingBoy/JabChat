using System.Xml.Serialization;

namespace XMPP.Core.Features;

[XmlRoot("mechanisms", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public class SaslFeature
{
  [XmlElement("mechanism")]
  public List<string> Mechanisms { get; set; } = new();
}