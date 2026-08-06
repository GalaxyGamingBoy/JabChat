using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.6
/// </summary>
[XmlRoot("internal-server-error", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record InternalServerError : IClientError
{
  public string What()
  {
    return
      "The server has experienced a misconfiguration or other internal error that prevents it from processing the stanza";
  }
};