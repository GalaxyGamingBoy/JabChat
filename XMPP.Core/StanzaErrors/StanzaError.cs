using System.Xml;
using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

public record StanzaError
{
  [XmlAttribute("type")]
  public ErrorType Type { get; init; }

  [XmlAnyElement]
  internal List<XmlElement> InternalErrors { get; init; } = [];

  [XmlIgnore]
  public List<IClientError> Errors { get; set; } = [];
}