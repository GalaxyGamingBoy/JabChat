using XMPP.Core.Address;

namespace XMPP.Core.SaslMechanisms;

public interface ISaslMechanism
{
  public string Mechanism { get; }
  public int Priority { get; }
  
  void BindClient(IXmppClient client);
  Task Use(XmppCredentials credentials);
}