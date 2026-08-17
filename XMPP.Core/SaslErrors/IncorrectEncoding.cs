using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.SaslErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-6.5.5
/// </summary>
[XmlRoot("incorrect-encoding", Namespace = "urn:ietf:params:xml:ns:xmpp-sasl")]
public record IncorrectEncoding : IClientError, IDefaultStanzaKey<IncorrectEncoding>
{
  public string What()
  {
    return
      "The data provided by the initiating entity could not be processed because the base 64 encoding is incorrect";
  }
};