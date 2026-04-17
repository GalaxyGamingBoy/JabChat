namespace XMPP.Core.Address;

public interface IXmppAddressValidator
{
  public Task<bool> IsXmppAddressValidAsync(XmppAddress address);
  public Task<bool> IsXmppAddressValidAsync(XmppAddress address, int timeout);
}