using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.14
/// </summary>
[XmlRoot("redirect", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record Redirect : IClientError
{
  public string What()
  {
    return
      "The recipient or server is redirecting requests for this information to another entity, typically in a temporary fashion (as opposed to the <gone/> error condition, which is used for permanent addressing failures)";
  }
};