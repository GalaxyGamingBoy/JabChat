using System.Xml;
using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

public record StanzaError
{
  [XmlAttribute("type")]
  public ErrorType Type { get; init; }

  [XmlAnyElement]
  public List<XmlElement> InternalErrors { get; set; } = [];

  [XmlIgnore]
  public List<IClientError> Errors { get; set; } = [];
}