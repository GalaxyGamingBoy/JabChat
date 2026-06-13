using System.Net.Sockets;
using FluentResults;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public class TcpXmppBackend : IXmppClientBackend
{
  private IXmppAddressProvider Provider { get; } = new XmppAddressProvider();
  
  private TcpClient? Client { get; set; } = null;
  public NetworkStream? Stream { get; private set; } = null;
  
  public void Dispose()
  {
    Stream?.Dispose();
    Client?.Dispose();
  }

  public async Task<Result> ConnectAsync(string host)
  {
    var addr = await Provider.GetAddressAsync(host);
    if (addr is null)
      return Result.Fail("No XMPP address found for Host");
    
    Stream = null;
    Client = new TcpClient();
    
    await Client.ConnectAsync(addr.Ip, addr.Port);
    
    Stream = Client.GetStream();
    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    
    return Result.Ok();
  }

  public void Disconnect()
  {
    Stream?.Close();
    Client?.Close();
    
    Stream = null;
    Client = null;
  }
}