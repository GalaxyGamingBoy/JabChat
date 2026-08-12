using System.Xml.Serialization;

namespace XMPP.Core.Presence;

public enum PresenceType
{
  [XmlEnum("none")]
  None,
  [XmlEnum("subscribe")]
  Subscribe,
  [XmlEnum("unsubscribe")]
  Unsubscribe,
  [XmlEnum("subscribed")]
  Subscribed,
  [XmlEnum("unsubscribed")]
  Unsubscribed,
  [XmlEnum("unavailable")]
  Unavailable,
  [XmlEnum("probe")]
  Probe,
  [XmlEnum("error")]
  Error,
}