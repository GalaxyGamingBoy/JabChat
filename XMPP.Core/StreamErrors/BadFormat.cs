using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.1
/// </summary>
[XmlRoot("bad-format", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record BadFormat : IClientError
{
  public string What()
  {
    return "The entity has sent XML that cannot be processed.";
  }
}