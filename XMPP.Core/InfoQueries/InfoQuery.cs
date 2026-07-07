using System.Xml.Serialization;

namespace XMPP.Core.InfoQueries;

[XmlRoot("iq", Namespace = "jabber:client")]
public record InfoQuery
{
  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("type")]
  public required InfoQueryType Type { get; set; }
  
  [XmlElement("bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
  public Bind? ResourceBind { get; set; }

  public record Bind
  {
    [XmlElement("resource")]
    public required string Resource { get; set; }
    
    [XmlElement("jid")]
    public string? Jid { get; set; }
  }
}