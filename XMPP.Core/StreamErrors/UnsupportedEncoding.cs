using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.22
/// </summary>
[XmlRoot("unsupported-encoding", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record UnsupportedEncoding : IClientError
{
  public string What()
  {
    return "The initiating entity has encoded the stream in an encoding that is not supported by the server";
  }
}