using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.4
/// </summary>
[XmlRoot("forbidden", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record Forbidden : IClientError
{
  public string What()
  {
    return
      "The requesting entity does not possess the necessary permissions to perform an action that only certain authorized roles or individuals are allowed to complete";
  }
};