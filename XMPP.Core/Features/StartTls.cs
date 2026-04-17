using System.Xml.Serialization;

namespace XMPP.Core.Features;

[XmlRoot("starttls", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
public class StartTlsFeature
{
  [XmlElement("required")]
  public object? Required { get; set; }
  
  [XmlIgnore]
  public bool IsRequired => Required != null;
}