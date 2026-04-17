using XMPP.Core.Address;

namespace XMPP.Core.Tests.Address;

public class XmppAddressProviderTests
{
  private readonly IXmppAddressProvider _provider = new XmppAddressProvider();

  [Fact]
  public async Task GetAddressAsync_ReturnsAddress_WhenExists()
  {
    var addr = await _provider.GetAddressAsync("jabberx.net");
    Assert.NotNull(addr);
    Assert.Equal("51.68.93.55", addr.Host);
    Assert.Equal(5222, addr.Port);
  }

  [Fact]
  public async Task GetAddressAsync_ReturnsNull_WhenNotExists()
  {
    (_provider as XmppAddressProvider)?.Timeout = 5;
    var addr = await _provider.GetAddressAsync("example.com");
    Assert.Null(addr);
  }
}