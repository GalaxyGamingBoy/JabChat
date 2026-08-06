using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.11
/// </summary>
[XmlRoot("not-authorized", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record NotAuthorized : IClientError
{
  public string What()
  {
    return
      "The sender needs to provide credentials before being allowed to perform the action, or has provided improper credentials";
  }
};