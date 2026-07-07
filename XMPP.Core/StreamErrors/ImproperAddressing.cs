using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.7
/// </summary>
[XmlRoot("improper-addressing", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record ImproperAddressing : IClientError
{
  public string What()
  {
    return "A stanza sent between two servers lacks a 'to' or 'from' attribute, the 'from' or 'to' attribute has no value, or the value violates the rules for XMPP addresses";
  }
}