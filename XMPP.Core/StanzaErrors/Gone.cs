using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.5
/// </summary>
[XmlRoot("gone", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record Gone : IClientError, IDefaultStanzaKey<Gone>
{
  public string What()
  {
    return
      "The recipient or server can no longer be contacted at this address, typically on a permanent basis";
  }
};