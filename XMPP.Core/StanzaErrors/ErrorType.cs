using System.Xml.Serialization;

namespace XMPP.Core;

public enum ErrorType
{
  [XmlEnum("auth")]
  Auth,
  [XmlEnum("cancel")]
  Cancel,
  [XmlEnum("continue")]
  Continue,
  [XmlEnum("modify")]
  Modify,
  [XmlEnum("wait")]
  Wait
}