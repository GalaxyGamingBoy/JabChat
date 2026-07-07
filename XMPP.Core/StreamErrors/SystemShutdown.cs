using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.20
/// </summary>
[XmlRoot("system-shutdown", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record SystemShutdown : IClientError
{
  public string What()
  {
    return "The server is being shut down and all active streams are being closed.";
  }
}