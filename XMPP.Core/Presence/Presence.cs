using System.Xml.Serialization;
using XMPP.Core.IM;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.Presence;

[XmlRoot("presence", Namespace = "jabber:client")]
public record Presence : XmppStanza
{
  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("to")]
  public string? To { get; set; }
  
  [XmlAttribute("from")]
  public string? From { get; set; }
  
  [XmlAttribute("type")]
  public PresenceType Type { get; set; }
  
  [XmlElement("show")]
  public PresenceShow Show { get; set; }
  
  [XmlElement("status")]
  public List<string>? Status { get; set; }
  
  [XmlElement("priority")]
  public int? Priority { get; set; }
  
  public bool ShouldSerializeType() => Type != PresenceType.None;
  public bool ShouldSerializeShow() => Show != PresenceShow.None;
}