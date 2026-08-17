using System.Xml.Serialization;

namespace XMPP.Core.IM;

[XmlRoot("query", Namespace = "jabber:iq:roster")]
public record InfoQueryRoster : IDefaultStanzaKey<InfoQueryRoster>
{
  [XmlAttribute("ver")]
  public string? Version { get; set; }

  [XmlElement("item")]
  public List<RosterItem> RosterItems { get; init; } = new();
}