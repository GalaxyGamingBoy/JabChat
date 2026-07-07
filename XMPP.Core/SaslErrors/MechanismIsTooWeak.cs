using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.9
/// </summary>
[XmlRoot("mechanism-too-week", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record MechanismTooWeak : IClientError
{
  public string What()
  {
    return
      "The mechanism requested by the initiating entity is weaker than server policy permits for that initiating entity";
  }
};