using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.10
/// </summary>
[XmlRoot("invalid-namespace", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record InvalidNamespace : IClientError, IDefaultStanzaKey<InvalidNamespace>
{
  public string What()
  {
    return "  The stream namespace name is something other than \"http://etherx.jabber.org/streams\"";
  }
}