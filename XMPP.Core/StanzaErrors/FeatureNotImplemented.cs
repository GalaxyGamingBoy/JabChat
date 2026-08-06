using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StanzaErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-8.3.3.3
/// </summary>
[XmlRoot("feature-not-implemented", Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas")]
public record FeatureNotImplemented : IClientError
{
  public string What()
  {
    return
      "The feature represented in the XML stanza is not implemented by the intended recipient or an intermediate server and therefore the stanza cannot be processed";
  }
};