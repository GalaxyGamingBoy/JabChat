using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.18
/// </summary>
[XmlRoot("resource-constraint", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record ResourceConstraint : IClientError
{
  public string What()
  {
    return
      "The server or recipient is busy or lacks the system resources necessary to service the request";
  }
};