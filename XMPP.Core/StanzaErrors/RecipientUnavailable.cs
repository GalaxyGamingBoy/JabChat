using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.13
/// </summary>
[XmlRoot("recipient-unavailable", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record RecipientUnavailable : IClientError
{
  public string What()
  {
    return
      "The intended recipient is temporarily unavailable, undergoing maintenance, etc.";
  }
};