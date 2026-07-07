using System.Xml.Serialization;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.8
/// </summary>
[XmlRoot("malformed-request", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record MalformedRequest : IClientError
{
  public string What()
  {
    return
      "The request is malformed";
  }
};