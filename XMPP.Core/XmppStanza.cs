using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core;

public record XmppStanza : XmppStanzaExtensions
{
  [XmlElement("error")]
  public StanzaError? StanzaError { get; init; }
}