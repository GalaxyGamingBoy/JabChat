using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.17
/// </summary>
[XmlRoot("remote-server-timeout", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record RemoteServerTimeout : IClientError
{
  public string What()
  {
    return
      "A remote server or service specified as part or all of the JID of the intended recipient (or needed to fulfill a request) was resolved but communications could not be established within a reasonable amount of time";
  }
};