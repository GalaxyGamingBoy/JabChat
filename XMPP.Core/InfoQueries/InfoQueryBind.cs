using System.Xml.Serialization;

namespace XMPP.Core.InfoQueries;

public record Bind
{
  [XmlElement("resource")]
  public required string Resource { get; init; }
    
  [XmlElement("jid")]
  public string? Jid { get; init; }
}