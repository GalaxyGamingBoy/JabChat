using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.16
/// </summary>
[XmlRoot("remote-server-not-found", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record RemoteServerNotFound : IClientError
{
  public string What()
  {
    return
      "A remote server or service specified as part or all of the JID of the intended recipient does not exist or cannot be resolved";
  }
};