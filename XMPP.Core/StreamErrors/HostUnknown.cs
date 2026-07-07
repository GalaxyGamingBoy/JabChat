using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.6
/// </summary>
[XmlRoot("host-unknown", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class HostUnknown : IClientError
{
  public string What()
  {
    return "The value of the 'to' attribute provided in the initial stream header does not correspond to an FQDN that is serviced by the receiving entity.";
  }
}