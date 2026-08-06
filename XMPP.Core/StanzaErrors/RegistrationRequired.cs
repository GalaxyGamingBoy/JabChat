using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.15
/// </summary>
[XmlRoot("registration-required", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record RegistrationRequired : IClientError
{
  public string What()
  {
    return
      "The requesting entity is not authorized to access the requested service because prior registration is necessary";
  }
};