using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.7
/// </summary>
[XmlRoot("invalid-mechanism", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record InvalidMechanism : IClientError, IDefaultStanzaKey<InvalidMechanism>
{
  public string What()
  {
    return
      "The initiating entity did not specify a mechanism, or requested a mechanism that is not supported by the receiving entity";
  }
};