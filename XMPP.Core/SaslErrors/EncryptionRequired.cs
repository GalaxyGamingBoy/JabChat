using System.Xml.Serialization;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.4
/// </summary>
[XmlRoot("encryption-required", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record EncryptionRequired : IClientError
{
  public string What()
  {
    return
      "The mechanism requested by the initiating entity cannot be used unless the confidentiality and integrity of the underlying stream are protected (typically via TLS)";
  }
};