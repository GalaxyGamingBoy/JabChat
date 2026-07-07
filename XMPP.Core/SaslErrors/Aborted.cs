using System.Xml.Serialization;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.1
/// </summary>
[XmlRoot("aborted", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record Aborted : IClientError
{
  public string What()
  {
    return
      "The receiving entity acknowledges that the authentication handshake has been aborted by the initiating entity";
  }
};