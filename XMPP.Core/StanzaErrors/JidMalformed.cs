using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.8
/// </summary>
[XmlRoot("jid-malformed", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record JidMalformed : IClientError
{
  public string What()
  {
    return
      "The sending entity has provided (e.g., during resource binding) or communicated (e.g., in the 'to' address of a stanza) an XMPP address or aspect thereof that violates the rules defined";
  }
};