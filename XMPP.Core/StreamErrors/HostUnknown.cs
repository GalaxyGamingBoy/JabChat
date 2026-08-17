using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.6
/// </summary>
[XmlRoot("host-unknown", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record HostUnknown : IClientError, IDefaultStanzaKey<HostUnknown>
{
  public string What()
  {
    return "The value of the 'to' attribute provided in the initial stream header does not correspond to an FQDN that is serviced by the receiving entity.";
  }
}