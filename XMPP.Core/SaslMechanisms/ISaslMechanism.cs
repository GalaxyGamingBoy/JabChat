using XMPP.Core.Backend;

namespace XMPP.Core.SaslMechanisms;

public interface ISaslMechanism
{
  public string Mechanism { get; }
  public int Priority { get; }
  
  void BindClient(IXmppClient client, IXmppClientBackend backend);
  Task Use(XmppCredentials credentials);
}