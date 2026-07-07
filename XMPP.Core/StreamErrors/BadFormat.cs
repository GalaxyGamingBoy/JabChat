using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.1
/// </summary>
[XmlRoot("bad-format", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class BadFormat : IClientError
{
  public string What()
  {
    return "The entity has sent XML that cannot be processed.";
  }
}