using System.Xml.Serialization;
using XMPP.Core.Errors;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.21
/// </summary>
[XmlRoot("undefined-condition", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public record UndefinedCondition : IClientError, IDefaultStanzaKey<UndefinedCondition>
{
  public string What()
  {
    return "The error condition is not one of those defined by the other conditions in this list; this error condition SHOULD NOT be used except in conjunction with an application-specific condition.";
  }
  
  [XmlText]
  public string Body { get; set; }
}