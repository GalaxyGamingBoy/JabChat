using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.5
/// </summary>
[XmlRoot("host-gone", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record HostGone : IClientError, IDefaultStanzaKey<HostGone>
{
  public string What()
  {
    return "The value of the 'to' attribute provided in the initial stream header corresponds to an FQDN that is no longer serviced by the receiving entity.";
  }
}