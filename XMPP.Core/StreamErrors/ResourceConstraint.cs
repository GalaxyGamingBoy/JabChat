using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.17
/// </summary>
[XmlRoot("resource-constraint", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record ResourceConstraint : IClientError
{
  public string What()
  {
    return "The server lacks the system resources necessary to service the stream.";
  }
}