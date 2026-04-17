namespace XMPP.Core.Address;

public record XmppAddress(string Host, string Ip, int Port);
public record XmppAddressSrv(string Host, int Port, int Priority, int Weight);