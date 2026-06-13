using XMPP.Core.Address;
using XMPP.Core.Backend;

namespace XMPP.Core;

public class XmppClientBuilder
{
  public required XmppCreds Credentials;
  public required string Host;
  public required XmppBackend Backend;

  public XmppClientBuilder UseCredentials(XmppCreds credentials)
  {
    this.Credentials = credentials;
    return this;
  }

  public XmppClientBuilder UseHost(string host)
  {
    this.Host =  host;
    return this;
  }

  public XmppClientBuilder UseBackend(XmppBackend backend)
  {
    this.Backend = backend;
    return this;
  }

  public XmppClientBuilder UseTcp()
  {
    this.Backend = XmppBackend.Tcp;
    return this;
  }

  public XmppClientBuilder UseWebsocket()
  {
    this.Backend = XmppBackend.Websocket;
    return this;
  }

  public IXmppClient Build()
  {
    return new XmppClient3()
    {
      Backend = GetBackend(),
      Credentials = Credentials,
      Host = Host,
    };
  }

  private IXmppClientBackend GetBackend() => Backend switch
  {
    XmppBackend.Tcp => new TcpXmppBackend(),
    XmppBackend.Websocket => throw new NotImplementedException(),
    _ => throw new ArgumentOutOfRangeException()
  };
}