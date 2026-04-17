using DnsClient;

namespace XMPP.Core.Address;

// todo: aaaa recs maybe
public class XmppAddressResolver : IXmppAddressResolver
{
  private readonly ILookupClient _lookup = new LookupClient();
  
  public async Task<List<XmppAddressSrv>> ResolveAddressFromSrvAsync(string host)
  {
    var srvClient = await _lookup.QueryAsync($"_xmpp-client._tcp.{host}.", QueryType.SRV);
    var records = srvClient.Answers.SrvRecords().ToList();
    return records.Select(e => new XmppAddressSrv(e.Target.Value, e.Port, e.Priority, e.Weight)).ToList();
  }

  public async Task<XmppAddress?> ResolveRootAddressAsync(string host)
  {
    var record = await _lookup.QueryAsync(host, QueryType.A);
    var aRecord = record.Answers.ARecords().FirstOrDefault();
    return aRecord is not null ? new XmppAddress(host, aRecord.Address.ToString(), 5222) : null;
  }

  public async Task<XmppAddress?> ResolveAddressAsync(XmppAddressSrv srv)
  {
    var record = await _lookup.QueryAsync(srv.Host, QueryType.A);
    var aRecord = record.Answers.ARecords().FirstOrDefault();
    return aRecord is not null ? new XmppAddress(srv.Host,aRecord.Address.ToString(), srv.Port) : null;
  }
}