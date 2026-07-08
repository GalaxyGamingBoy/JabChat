using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.1
/// </summary>
[XmlRoot("bad-request", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record BadRequest : IClientError
{
  public string What()
  {
    return
      "The sender has sent a stanza containing XML that does not conform to the appropriate schema or that cannot be processed";
  }
};