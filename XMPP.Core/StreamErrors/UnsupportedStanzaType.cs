using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.24
/// </summary>
[XmlRoot("unsupported-stanza-type", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record UnsupportedStanzaType : IClientError
{
  public string What()
  {
    return "The initiating entity has sent a first-level child of the stream that is not supported by the server, either because the receiving entity does not understand the namespace or because the receiving entity does not understand the element name for the applicable namespace";
  }
}