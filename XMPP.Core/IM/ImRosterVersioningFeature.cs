using System.Xml.Serialization;

namespace XMPP.Core.IM;

[XmlRoot("ver", Namespace = "urn:xmpp:features:rosterver")]
public record ImRosterVersioningFeature : IDefaultStanzaKey<ImRosterVersioningFeature>;