using XMPP.Core.Address;

namespace XMPP.Core.Tests.Address;

public class XmppAddressValidatorTests
{
  private readonly IXmppAddressValidator _validator = new XmppAddressValidator();
  
  [Fact]
  public async Task IsXmppAddressValidAsync_ReturnsTrue_WhenAddressValid()
  {
    var address = new XmppAddress("jabberx.net", "51.68.93.55", 5222);
    Assert.True(await _validator.IsXmppAddressValidAsync(address));
  }

  [Fact]
  public async Task IsXmppAddressValidAsync_ReturnsFalse_WhenAddressInvalid()
  {
    var address = new XmppAddress("jabberx.net", "51.68.93.55", 5000);
    Assert.False(await _validator.IsXmppAddressValidAsync(address));
  }
}