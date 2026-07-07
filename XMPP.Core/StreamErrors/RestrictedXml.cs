using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.18
/// </summary>
[XmlRoot("restricted-xml", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class RestrictedXml : IClientError
{
  public string What()
  {
    return "The entity has attempted to send restricted XML features such as a comment, processing instruction, DTD subset, or XML entity reference";
  }
}