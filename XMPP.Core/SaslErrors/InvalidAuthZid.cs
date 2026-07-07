using System.Xml.Serialization;
using XMPP.Core.ClientErrors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.6
/// </summary>
[XmlRoot("invalid-authzid", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record InvalidAuthZid : IClientError
{
  public string What()
  {
    return
      "The authzid provided by the initiating entity is invalid, either because it is incorrectly formatted or because the initiating entity does not have permissions to authorize that ID";
  }
};