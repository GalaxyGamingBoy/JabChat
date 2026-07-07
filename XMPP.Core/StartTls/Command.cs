using System.Xml.Serialization;

namespace XMPP.Core.StartTls;

[XmlRoot("starttls", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
public record Command;