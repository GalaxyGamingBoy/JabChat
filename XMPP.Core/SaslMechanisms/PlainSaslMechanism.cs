using System.Xml;
using System.Xml.Linq;
using XMPP.Core.Address;

namespace XMPP.Core.SaslMechanisms;

public class PlainSaslMechanism : ISaslMechanism
{
  private IXmppClient _client = null!;
  
  public string Mechanism => "PLAIN";
  public int Priority => 500;

  public void BindClient(IXmppClient client)
  {
    _client = client;
  }
  
  private async void OnSuccessReceived(object sender, object? successMessageReceived)
  {
    await _client.StopBackgroundService();
    await _client.SaslCompleted();
    _client.StartBackgroundService();
    _client.ReadLock.Release();
  }

  public async Task Use(XmppCreds credentials)
  {
    _client.RegisterUnexpectedStanza<SaslSuccess>(OnSuccessReceived);
    
    var localpart = credentials.Jid.Split("@")[0];
    var message = $"\0{localpart}\0{credentials.Password}";
    
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "auth");
    element.SetAttributeValue("mechanism", Mechanism);
    
    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(System.Convert.ToBase64String(bytes));
    
    await _client.SendStanzaAsync(element);
  }
}