using System.Xml.Serialization;

namespace XMPP.Core.Messages;

public enum MessageType
{
  [XmlEnum("normal")]
  Normal,
  [XmlEnum("chat")]
  Chat,
  [XmlEnum("error")]
  Error,
  [XmlEnum("groupchat")]
  GroupChat,
  [XmlEnum("headline")]
  Headline
}