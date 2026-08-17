using System.Xml.Serialization;

namespace XMPP.Core.StartTls;

[XmlRoot("proceed", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
public record Proceed : IDefaultStanzaKey<Proceed>;