using System.Xml.Serialization;

namespace XMPP.Core;

public enum InfoQueryType
{
  [XmlEnum("get")]
  Get,
  
  [XmlEnum("set")]
  Set,
  
  [XmlEnum("result")]
  Result,
  
  [XmlEnum("error")]
  Error
}