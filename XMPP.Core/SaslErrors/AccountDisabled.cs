using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.2
/// </summary>
[XmlRoot("account-disabled", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record AccountDisabled : IClientError
{
  public string What()
  {
    return
      "The account of the initiating entity has been temporarily disabled";
  }
};