using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.7
/// </summary>
[XmlRoot("item-not-found", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record ItemNotFound : IClientError
{
  public string What()
  {
    return
      "The addressed JID or item requested cannot be found";
  }
};