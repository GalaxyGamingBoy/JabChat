using System.Xml.Serialization;

namespace XMPP.Core.IM;

public enum PresenceShow
{
  None,
  [XmlEnum("chat")]
  Active,
  [XmlEnum("away")]
  Away,
  [XmlEnum("dnd")]
  DoNotDisturb,
  [XmlEnum("xa")]
  ExtendedAway
}