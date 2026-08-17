using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.3
/// </summary>
[XmlRoot("credentials-expired", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record CredentialsExpired : IClientError, IDefaultStanzaKey<CredentialsExpired>
{
  public string What()
  {
    return
      "The authentication failed because the initiating entity provided credentials that have expired";
  }
};