using XMPP.Core.Address;

namespace XMPP.Core.Tests.Address;

// resolve from jabberx.net.
// public Task<List<XmppAddressSrv>> ResolveAddressFromSrvAsync(string host);
// public Task<XmppAddress?> ResolveAddressFromRootAsync(string host);
// public Task<XmppAddress?> ResolveAddress(XmppAddressSrv address);

public class XmppAddressResolverTests
{
  private readonly IXmppAddressResolver _resolver = new XmppAddressResolver();

  [Fact]
  public async Task ResolveAddressFromSrvAsync_ReturnsAddresses_WhenExists()
  {
    var recs = await _resolver.ResolveAddressFromSrvAsync("jabberx.net");
    Assert.Equal(recs, [
      new XmppAddressSrv("jabberx.net.", 5222, 5, 0)
    ]);
  }

  [Fact]
  public async Task ResolveAddressFromSrvAsync_ReturnsEmptyArray_WhenNotExists()
  {
    var recs = await _resolver.ResolveAddressFromSrvAsync("example.com");
    Assert.Empty(recs);
  }

  [Fact]
  public async Task ResolveRootAddressAsync_ReturnsAddress_WhenExists()
  {
    var rec = await _resolver.ResolveRootAddressAsync("jabberx.net.");
    Assert.NotNull(rec);
    
    Assert.Equal("jabberx.net.", rec.Host);
    Assert.Equal("51.68.93.55", rec.Ip);
  }

  [Fact]
  public async Task ResolveAddressAsync_ReturnsAddress_WhenExists()
  {
    var addr = new XmppAddressSrv("jabberx.net.", 5222, 5, 0);
    var rec = await _resolver.ResolveAddressAsync(addr);
    
    Assert.NotNull(rec);
    Assert.Equal("jabberx.net.", rec.Host);
    Assert.Equal("51.68.93.55", rec.Ip);
    Assert.Equal(addr.Port, rec.Port);
  }
}