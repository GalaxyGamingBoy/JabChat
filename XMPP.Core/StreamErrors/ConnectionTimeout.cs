using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.4
/// </summary>
[XmlRoot("connection-timeout", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record ConnectionTimeout : IClientError
{
  public string What()
  {
    return
      "One party is closing the stream because it has reason to believe that the other party has permanently lost the ability to communicate over the stream.";
  }
}