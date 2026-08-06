using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.3
/// </summary>
[XmlRoot("conflict", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record Conflict : IClientError
{
  public string What()
  {
    return "The server either (1) is closing the existing stream for this entity because a new stream has been initiated that conflicts with the existing stream, or (2) is refusing a new stream for this entity because allowing the new stream would conflict with an existing stream";
  }
}