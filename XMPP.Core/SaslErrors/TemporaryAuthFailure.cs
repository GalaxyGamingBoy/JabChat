using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.11
/// </summary>
[XmlRoot(" temporary-auth-failure", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record TemporaryAuthFailure : IClientError
{
  public string What()
  {
    return
      "The authentication failed because of a temporary error condition within the receiving entity, and it is advisable for the initiating entity to try again later";
  }
};