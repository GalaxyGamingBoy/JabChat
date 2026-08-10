using System.Xml.Serialization;

namespace XMPP.Core.Presence;

public enum PresenceType
{
  [XmlEnum("subscribed")]
  Subscribed,
  [XmlEnum("unsubscribed")]
  Unsubscribed,
}