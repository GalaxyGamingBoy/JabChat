namespace XMPP.Core.Address;

public interface IXmppAddressSelector
{
  public List<XmppAddressSrv> Select(IEnumerable<XmppAddressSrv> addresses);
}