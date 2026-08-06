using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.15
/// </summary>
[XmlRoot("remote-connection-failed", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record RemoteConnectionFailed : IClientError
{
  public string What()
  {
    return "The server is unable to properly connect to a remote entity that is needed for authentication or authorization";
  }
}