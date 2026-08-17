using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.2
/// </summary>
[XmlRoot("bad-namespace-prefix", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record BadNamespacePrefix : IClientError, IDefaultStanzaKey<BadNamespacePrefix>
{
  public string What()
  {
    return "The entity has sent a namespace prefix that is unsupported, or has sent no namespace prefix on an element that needs such a prefix";
  }
}