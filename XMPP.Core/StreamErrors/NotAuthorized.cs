using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.12
/// </summary>
[XmlRoot("not-authorized", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record NotAuthorized : IClientError
{
  public string What()
  {
    return "The entity has attempted to send XML stanzas or other outbound data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation";
  }
}