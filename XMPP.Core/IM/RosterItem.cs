using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace XMPP.Core.IM;

public record RosterItem
{
  [XmlAttribute("jid")]
  public required string Jid { get; init; }
  
  [XmlAttribute("name")]
  public string? Name { get; init; }

  [XmlAttribute("subscription")]
  public RosterItemSubscription Subscription { get; init; } = RosterItemSubscription.None;
  
  [XmlAttribute("approved")]
  public bool Approved { get; init; }

  [XmlElement("group")]
  public List<string> Groups { get; init; } = [];
}
