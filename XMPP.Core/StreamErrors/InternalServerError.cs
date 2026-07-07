using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.8
/// </summary>
[XmlRoot("internal-server-error", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record InternalServerError : IClientError
{
  public string What()
  {
    return "The server has experienced a misconfiguration or other internal error that prevents it from servicing the stream.";
  }
}