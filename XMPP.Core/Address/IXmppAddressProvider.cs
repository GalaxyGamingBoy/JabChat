namespace XMPP.Core.Address;

public interface IXmppAddressProvider
{
  public Task<XmppAddress?> GetAddressAsync(string host);
}