using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.19
/// </summary>
[XmlRoot("see-other-host", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record SeeOtherHost : IClientError, IDefaultStanzaKey<SeeOtherHost>
{
  public string What()
  {
    return "The server will not provide service to the initiating entity but is redirecting traffic to another host under the administrative control of the same service provider.";
  }

  [XmlText]
  public required string Host { get; set; }
}