using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.Messages;

[XmlRoot("message", Namespace = "jabber:client")]
public record Message
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
  
  [XmlElement("error")]
  public StanzaError? StanzaError;

  [XmlElement("thread")]
  public MessageThread? Thread;
};