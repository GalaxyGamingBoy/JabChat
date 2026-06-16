using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.16
/// </summary>
[XmlRoot("reset", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class Reset : IStreamError
{
  public string What()
  {
    return "The server is closing the stream because it has new (typically security-critical) features to offer";
  }
}