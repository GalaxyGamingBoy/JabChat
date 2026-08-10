using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.Presence;

[XmlRoot("presence", Namespace = "jabber:client")]
public record Presence
{
  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("to")]
  public string? To { get; set; }
  
  [XmlAttribute("from")]
  public string? From { get; set; }
  
  [XmlAttribute("type")]
  public PresenceType Type { get; set; }
  
  [XmlElement("error")]
  public StanzaError? StanzaError { get; set; }
}