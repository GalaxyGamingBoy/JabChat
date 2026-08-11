using System.Xml.Serialization;

namespace XMPP.Core.Presence;

public enum PresenceType
{
  [XmlEnum("subscribe")]
  Subscribe,
  [XmlEnum("unsubscribe")]
  Unsubscribe,
  [XmlEnum("subscribed")]
  Subscribed,
  [XmlEnum("unsubscribed")]
  Unsubscribed,
}