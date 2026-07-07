using System.Xml.Serialization;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.10
/// </summary>
[XmlRoot("not-authorized", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record NotAuthorized : IClientError
{
  public string What()
  {
    return
      "The authentication failed because the initiating entity did not provide proper credentials, or because some generic authentication failure has occurred but the receiving entity does not wish to disclose specific information about the cause of the failure";
  }
};