using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.Messages;

[XmlRoot("message", Namespace = "jabber:client")]
public record Message
{
  [XmlAttribute("to")]
  public string? To;
  
  [XmlAttribute("from")]
  public string? From;
  
  [XmlAttribute("id")]
  public string? Id;
  
  [XmlElement("body")]
  public required string Body;

  [XmlElement("error")]
  public StanzaError? StanzaError;
};