using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.10
/// </summary>
[XmlRoot("not-allowed", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record NotAllowed : IClientError
{
  public string What()
  {
    return
      "The recipient or server does not allow any entity to perform the action";
  }
};