using System.Net.Sockets;
using FluentResults;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public class TcpXmppBackend : IXmppClientBackend
{
  private IXmppAddressProvider Provider { get; } = new XmppAddressProvider();
  
  private TcpClient? Client { get; set; } = null;
  public NetworkStream? Stream { get; private set; } = null;

  public void RefreshNetworkStream()
  {
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs { Stream = Stream });
  }

  public void OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs)
  {
    if (eventArgs.Feature is Features.StartTlsFeature)
      Console.WriteLine("Attempting to upgrade session to TLS");
  }

  public event EventHandler<NetworkStreamUpdatedEventArgs>? NetworkStreamUpdated;

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
    
    return await ConnectAsync(addr); 
  }

  public async Task<Result> ConnectAsync(XmppAddress address)
  {
    Stream = null;
    Client = new TcpClient();
    
    await Client.ConnectAsync(address.Ip, address.Port);
    
    Stream = Client.GetStream();
    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs { Stream = Stream });
    
    return Result.Ok();
  }

  public void Disconnect()
  {
    Stream?.Close();
    Client?.Close();
    
    Stream = null;
    Client = null;
    
    NetworkStreamUpdated?.Invoke(this, new  NetworkStreamUpdatedEventArgs { Stream = null });
  }
}
