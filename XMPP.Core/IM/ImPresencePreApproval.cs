using System.Xml.Serialization;

namespace XMPP.Core.IM;

[XmlRoot("sub", Namespace = "urn:xmpp:features:pre-approval")]
public record ImPresencePreApproval : IDefaultStanzaKey<ImPresencePreApproval>;