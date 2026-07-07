using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.Backend;

namespace XMPP.Core;

public class XmppClientBuilder
{
  private string? _host;
  private XmppAddress?  _address;
  
  private XmppBackend _backend = XmppBackend.Tcp;
  
  private string? _username;
  private string? _password;
  private string _resource = Guid.NewGuid().ToString();
  
  private bool _forceTls;
  
  private XmppAddressProvider _addressProvider = new XmppAddressProvider();

  public XmppClientBuilder UseHost(string host)
  {
    this._host =  host;
    return this;
  }

  public XmppClientBuilder UseAddress(XmppAddress address)
  {
    this._address = address;
    _host = address.Host;
    return this;
  }

  public XmppClientBuilder UseBackend(XmppBackend backend)
  {
    this._backend = backend;
    return this;
  }

  public XmppClientBuilder UseTcp()
  {
    this._backend = XmppBackend.Tcp;
    return this;
  }

  public XmppClientBuilder UseWebsocket()
  {
    this._backend = XmppBackend.Websocket;
    return this;
  }

  public XmppClientBuilder UseUsername(string username)
  {
    this._username = username;
    return this;
  }

  public XmppClientBuilder UsePassword(string password)
  {
    this._password = password;
    return this;
  }

  public XmppClientBuilder UseResourceForBinding(string resource)
  {
    this._resource = resource;
    return this;
  }

  public XmppClientBuilder ForceTls()
  {
    this._forceTls = true;
    return this;
  }

  public async Task<Result<IXmppClient>> BuildAsync()
  {
    if (_host is null)
      return Result.Fail("Address or Host not specified.");
    
    if (_username is null)
      return Result.Fail("Username not specified.");
    
    if (_password is null)
      return Result.Fail("Password not specified.");
    
    if (_address is null)
    {
      var address = await _addressProvider.GetAddressAsync(_host!);
      if (address is null)
        return Result.Fail("No XMPP address was found");
    
      _address = address;
    }

    var jid = new XmppJid()
    {
      LocalPart = _username,
      DomainPart = _host,
      Resource = _resource
    };

    return new XmppClient3(GetBackend())
    {
      Credentials = new XmppCredentials(jid, _password),
      Address = _address,
    };
  }

  private IXmppClientBackend GetBackend() => _backend switch
  {
    XmppBackend.Tcp => new TcpXmppBackend(this._forceTls),
    XmppBackend.Websocket => throw new NotImplementedException(),
    _ => throw new ArgumentOutOfRangeException()
  };
}