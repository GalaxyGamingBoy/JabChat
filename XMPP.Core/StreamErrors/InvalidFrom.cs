using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.9
/// </summary>
[XmlRoot("invalid-from", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class InvalidFrom : IStreamError
{
  public string What()
  {
    return "The data provided in a 'from' attribute does not match an authorized JID or validated domain as negotiated (1) between two servers using SASL or Server Dialback, or (2) between a client and a server via SASL authentication and resource binding.";
  }
}