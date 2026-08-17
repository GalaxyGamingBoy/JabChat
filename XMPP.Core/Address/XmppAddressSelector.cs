namespace XMPP.Core.Address;

public class XmppAddressSelector(Random? random) : IXmppAddressSelector
{
  private Random Random { get; } = random ?? new Random();

  public List<XmppAddressSrv> Select(IEnumerable<XmppAddressSrv> addresses)
  {
    var recs = addresses
      .GroupBy(a => a.Priority)
      .OrderBy(g => g.Key)
      .Select(g => g.ToList())
      .ToList();
    List<XmppAddressSrv> order = [];

    foreach (var group in recs)
    {
      var l = group.ToList();
      while (l.Count > 0)
      {
        var weights = l.Sum(a => a.Weight);
        var weight = weights > 0
          ? Random.Next(weights)
          : Random.Next(l.Count);

        var currentWeight = 0;
        for (var i = 0; i < l.Count; i++)
        {
          var a = l[i];
          currentWeight += weights > 0 ? a.Weight : 1;
          if (currentWeight < weight) continue;
          
          order.Add(a);
          l.RemoveAt(i);
          break;
        }
      }
    }

    return order;
  }
}