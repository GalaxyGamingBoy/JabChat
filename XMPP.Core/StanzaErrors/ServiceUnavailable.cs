using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.19
/// </summary>
[XmlRoot("service-unavailable", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record ServiceUnavailable : IClientError, IDefaultStanzaKey<ServiceUnavailable>
{
  public string What()
  {
    return
      "The server or recipient does not currently provide the requested service";
  }
};