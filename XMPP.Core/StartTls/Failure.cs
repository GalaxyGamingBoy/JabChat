using System.Xml.Serialization;
using XMPP.Core.StreamErrors;

namespace XMPP.Core.StartTls;

[XmlRoot("failure", Namespace = "urn:ietf:params:xml:ns:xmpp-tls")]
public class Failure : IClientError
{
  public string What()
  {
    return "There was a failure with STARTTLS";
  }
};