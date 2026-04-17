using XMPP.Core.Address;

namespace XMPP.Core.Tests.Address;

public class XmppAddressSelectorTests
{
  private readonly IXmppAddressSelector _selector = new XmppAddressSelector(new Random(0));
  
  [Fact]
  public void Select_ReturnsWeightedRecords_WhenPriorityOrdered()
  {
    List<XmppAddressSrv> srvs =
    [
      new("1.xmpp.example.com", 1, 10, 60),
      new("2.xmpp.example.com", 2, 10, 20),
      new("3.xmpp.example.com", 3, 10, 20),
      new("4.xmpp.example.com", 4, 20, 0),
      new("5.xmpp.example.com", 5, 20, 50),
      new("6.xmpp.example.com", 6, 30, 100),
      new("7.xmpp.example.com", 7, 30, 0),
      new("8.xmpp.example.com", 8, 40, 10),
      new("9.xmpp.example.com", 9, 40, 40),
      new("10.xmpp.example.com", 10, 40, 50)
    ];
    
    List<XmppAddressSrv> sorted =
    [
      new("2.xmpp.example.com", 2, 10, 20),
      new("3.xmpp.example.com", 3, 10, 20),
      new("1.xmpp.example.com", 1, 10, 60),
      new("5.xmpp.example.com", 5, 20, 50),
      new("4.xmpp.example.com", 4, 20, 0),
      new("6.xmpp.example.com", 6, 30, 100),
      new("7.xmpp.example.com", 7, 30, 0),
      new("9.xmpp.example.com", 9, 40, 40),
      new("10.xmpp.example.com", 10, 40, 50),
      new("8.xmpp.example.com", 8, 40, 10)
    ]; 
    
    Assert.Equal(_selector.Select(srvs), sorted);
  } 
      
  [Fact]
  public void Select_ReturnsWeightedRecords_WhenPriorityRandom()
  {
    List<XmppAddressSrv> srvs =
    [
      new("1.xmpp.example.com", 1, 20, 0),
      new("2.xmpp.example.com", 2, 20, 50),
      new("3.xmpp.example.com", 3, 10, 60),
      new("4.xmpp.example.com", 4, 10, 20),
      new("5.xmpp.example.com", 5, 10, 20),
      new("6.xmpp.example.com", 6, 30, 100),
      new("7.xmpp.example.com", 7, 30, 0),
      new("8.xmpp.example.com", 8, 40, 10),
      new("9.xmpp.example.com", 9, 40, 40),
      new("10.xmpp.example.com", 10, 40, 50)
    ];
    
    List<XmppAddressSrv> sorted =
    [
      new("4.xmpp.example.com", 4, 10, 20),
      new("5.xmpp.example.com", 5, 10, 20),
      new("3.xmpp.example.com", 3, 10, 60),
      new("2.xmpp.example.com", 2, 20, 50),
      new("1.xmpp.example.com", 1, 20, 0),
      new("6.xmpp.example.com", 6, 30, 100),
      new("7.xmpp.example.com", 7, 30, 0),
      new("9.xmpp.example.com", 9, 40, 40),
      new("10.xmpp.example.com", 10, 40, 50),
      new("8.xmpp.example.com", 8, 40, 10),
    ];

    Assert.Equal(_selector.Select(srvs), sorted);
  }

  [Fact]
  public void Select_ReturnsRandomRecords_WhenAllWeightsAre0()
  {
    List<XmppAddressSrv> srvs =
    [
      new("1.xmpp.example.com", 1, 10, 0),
      new("2.xmpp.example.com", 2, 10, 0),
      new("3.xmpp.example.com", 3, 10, 0),
      new("4.xmpp.example.com", 4, 10, 0),
    ];

    List<XmppAddressSrv> sorted =
    [
      new("2.xmpp.example.com", 2, 10, 0),
      new("3.xmpp.example.com", 3, 10, 0),
      new("1.xmpp.example.com", 1, 10, 0),
      new("4.xmpp.example.com", 4, 10, 0)
    ];

    Assert.Equal(_selector.Select(srvs), sorted);
  }
}