using System.Xml.Serialization;

namespace XMPP.Core.StreamErrors;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc6120#section-4.9.3.21
/// </summary>
[XmlRoot("undefined-condition", Namespace = "urn:ietf:params:xml:ns:xmpp-streams")]
public class UndefinedCondition : IClientError
{
  public string What()
  {
    return "The error condition is not one of those defined by the other conditions in this list; this error condition SHOULD NOT be used except in conjunction with an application-specific condition.";
  }
  
  public string Body { get; set; }
}