using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.23
/// </summary>
[XmlRoot("unsupported-feature", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record UnsupportedFeature : IClientError, IDefaultStanzaKey<UnsupportedFeature>
{
  public string What()
  {
    return "The receiving entity has advertised a mandatory-to-negotiate stream feature that the initiating entity does not support, and has offered no other mandatory-to-negotiate feature alongside the unsupported feature.";
  }
}