namespace XMPP.Core.Address;

public interface IXmppAddressResolver
{
  public Task<List<XmppAddressSrv>> ResolveAddressFromSrvAsync(string host);
  public Task<XmppAddress?> ResolveRootAddressAsync(string host);
  public Task<XmppAddress?> ResolveAddressAsync(XmppAddressSrv address);
}