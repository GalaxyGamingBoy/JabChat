using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.2
/// </summary>
[XmlRoot("conflict", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record Conflict : IClientError
{
  public string What()
  {
    return
      "Access cannot be granted because an existing resource exists with the same name or address";
  }
};