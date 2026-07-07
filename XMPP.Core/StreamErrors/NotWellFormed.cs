using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.13
/// </summary>
[XmlRoot("not-well-formed", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record NotWellFormed : IClientError
{
  public string What()
  {
    return "The initiating entity has sent XML that violates the well-formedness rules of XML or XML-NAMES.";
  }
}