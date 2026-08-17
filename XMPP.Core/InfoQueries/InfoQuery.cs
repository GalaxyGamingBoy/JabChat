using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.InfoQueries;

[XmlRoot("iq", Namespace = "jabber:client")]
public record InfoQuery : StanzaExtensions
{
  public InfoQuery()
  {
  }

  [SetsRequiredMembers]
  public InfoQuery(InfoQueryType type)
  {
    Type = type;
  }

  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("type")]
  public required InfoQueryType Type { get; init; }
  
  [XmlAttribute("to")]
  public string? To;
  
  [XmlAttribute("from")]
  public string? From;
  
  [XmlElement("error")]
  public StanzaError? StanzaError { get; init; }
  
  [XmlElement("bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
  public Bind? ResourceBind { get; init; }
}