using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.21
/// </summary>
[XmlRoot("undefined-condition", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record UndefinedCondition : IClientError
{
  public string What()
  {
    return
      "The error condition is not one of those defined by the other conditions in this list";
  }
};