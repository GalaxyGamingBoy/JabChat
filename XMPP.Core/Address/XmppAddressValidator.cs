using System.Net.Sockets;

namespace XMPP.Core.Address;

public class XmppAddressValidator : IXmppAddressValidator
{
  public async Task<bool> IsXmppAddressValidAsync(XmppAddress address, int timeout)
  {
    using var client = new TcpClient();

    try
    {
      await client.ConnectAsync(address.Ip, address.Port).WaitAsync(TimeSpan.FromSeconds(timeout));
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public Task<bool> IsXmppAddressValidAsync(XmppAddress address)
  {
    return  IsXmppAddressValidAsync(address, 40);
  }
}