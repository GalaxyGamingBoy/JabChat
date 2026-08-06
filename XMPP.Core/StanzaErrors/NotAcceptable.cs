using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.9
/// </summary>
[XmlRoot("not-acceptable", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record NotAcceptable : IClientError
{
  public string What()
  {
    return
      "The recipient or server understands the request but cannot process it because the request does not meet criteria defined by the recipient or server";
  }
};