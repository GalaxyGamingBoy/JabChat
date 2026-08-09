using System.Xml.Serialization;

namespace XMPP.Core.IM;

[XmlRoot("query", Namespace = "jabber:iq:roster")]
public record InfoQueryRoaster
{
  [XmlAttribute("ver")]
  public string? Version { get; private set; }

  [XmlElement("item")]
  private List<InfoQueryRoasterItem> RosterItems { get; init; } = new();
};

public record InfoQueryRoasterItem
{
  [XmlAttribute("jid")]
  public required string Jid { get; init; }
}