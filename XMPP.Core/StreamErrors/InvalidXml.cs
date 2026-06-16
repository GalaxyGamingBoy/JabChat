using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.11
/// </summary>
[XmlRoot("invalid-xml", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class InvalidXml : IStreamError
{
  public string What()
  {
    return "The entity has sent invalid XML over the stream to a server that performs validation";
  }
}