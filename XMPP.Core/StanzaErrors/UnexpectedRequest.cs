using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.22
/// </summary>
[XmlRoot("undefined-condition", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record UnexpectedRequest : IClientError, IDefaultStanzaKey<UnexpectedRequest>
{
  public string What()
  {
    return
      "The recipient or server understood the request but was not expecting it at this time";
  }
};