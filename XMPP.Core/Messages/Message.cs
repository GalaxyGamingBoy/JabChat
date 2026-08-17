using System.Xml.Serialization;

namespace XMPP.Core.Messages;

[XmlRoot("message", Namespace = "jabber:client")]
public record Message : XmppStanza
{
  
  [XmlAttribute("id")]
  public string? Id;
  
  [XmlAttribute("to")]
  public required string To;
  
  [XmlAttribute("from")]
  public required string From;
  
  [XmlAttribute("type")]
  public MessageType Type;
  
  [XmlElement("body")]
  public List<string> Body = [];
  
  [XmlElement("subject")]
  public List<string> Subject = [];
  
  [XmlElement("thread")]
  public MessageThread? Thread;
};