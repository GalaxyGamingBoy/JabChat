using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.25
/// </summary>
[XmlRoot("unsupported-version", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record UnsupportedVersion : IClientError
{
  public string What()
  {
    return "The 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server.";
  }
}