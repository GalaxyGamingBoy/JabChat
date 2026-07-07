using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.5
/// </summary>
[XmlRoot("host-gone", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class HostGone : IClientError
{
  public string What()
  {
    return "The value of the 'to' attribute provided in the initial stream header corresponds to an FQDN that is no longer serviced by the receiving entity.";
  }
}