using System.Xml.Serialization;

namespace XMPP.Core.InfoQueries;

[XmlRoot("iq", Namespace = "jabber:client")]
public record InfoQuery : XmppStanza
{
  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("type")]
  public required InfoQueryType Type { get; init; }
  
  [XmlAttribute("to")]
  public string? To;
  
  [XmlAttribute("from")]
  public string? From;
  
  [XmlElement("bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
  public Bind? ResourceBind { get; init; }
}