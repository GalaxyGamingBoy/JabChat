using System.Xml.Linq;
using XMPP.Core.Backend;

namespace XMPP.Core.SaslMechanisms;

public sealed class PlainSaslMechanism : ISaslMechanism
{
  private IXmppClient _client = null!;
  
  public string Mechanism => "PLAIN";
  public int Priority => 600;

  public void BindClient(IXmppClient client, IXmppClientBackend backend)
  {
    _client = client;
  }
  
  private async Task OnSuccessReceived(object sender, object? successMessageReceived)
  {
    await _client.StopBackgroundService();
    await _client.SaslCompleted();
    _client.StartBackgroundService();
    _client.ReadLock.Release();
    
    _client.UnregisterUnexpectedStanza<SaslSuccess>();
  }

  public async Task Use(XmppCredentials credentials)
  {
    _client.RegisterUnexpectedStanza<SaslSuccess>(OnSuccessReceived);
    
    var message = $"\0{credentials.Jid.LocalPart}\0{credentials.Password}";
    
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "auth");
    element.SetAttributeValue("mechanism", Mechanism);
    
    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(Convert.ToBase64String(bytes));
    
    await _client.SendStanzaAsync(element);
  }
}