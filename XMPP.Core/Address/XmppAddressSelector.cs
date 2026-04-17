namespace XMPP.Core.Address;

public class XmppAddressSelector(Random? random) : IXmppAddressSelector
{
  private Random _random { get; } = random ?? new Random();

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
          ? _random.Next(weights)
          : _random.Next(l.Count);

        var current_weight = 0;
        for (var i = 0; i < l.Count; i++)
        {
          var a = l[i];
          current_weight += weights > 0 ? a.Weight : 1;
          if (current_weight < weight) continue;
          
          order.Add(a);
          l.RemoveAt(i);
          break;
        }
      }
    }

    return order;
  }
}