namespace XMPP.Core.Address;

public class XmppAddressProvider : IXmppAddressProvider
{
  private readonly IXmppAddressResolver _resolver = new XmppAddressResolver();
  private readonly IXmppAddressValidator _validator = new XmppAddressValidator();
  private readonly IXmppAddressSelector _selector = new XmppAddressSelector(null);

  public int Timeout { get; set; } = 40;
  
  public async Task<XmppAddress?> GetAddressAsync(string host)
  {
    var addr = _selector.Select(await _resolver.ResolveAddressFromSrvAsync(host));

    foreach (var srv in addr)
    {
      var r = await _resolver.ResolveAddressAsync(srv);
      if (r is null) continue;
      
      if (await _validator.IsXmppAddressValidAsync(r, Timeout))
        return r;
    }
    
    var root = await _resolver.ResolveRootAddressAsync(host);
    if (root is null) return null;

    if (await _validator.IsXmppAddressValidAsync(root, Timeout))
      return root;
    
    return null;
  }
}