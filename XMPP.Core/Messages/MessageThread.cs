using System.Xml.Serialization;

namespace XMPP.Core.Messages;

public record MessageThread
{
  [XmlElement("parent")]
  public string? Parent;

  [XmlText]
  public required string Body;
};