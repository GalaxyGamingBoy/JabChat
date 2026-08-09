using System.Xml.Serialization;

namespace XMPP.Core.IM;

public enum RosterItemSubscription
{
  [XmlEnum("none")]
  None,
  [XmlEnum("to")]
  To,
  [XmlEnum("from")]
  From,
  [XmlEnum("both")]
  Both,
  [XmlEnum("remove")]
  Remove
}