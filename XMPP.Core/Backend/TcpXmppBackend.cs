using System.Net.Security;
using System.Net.Sockets;
using FluentResults;
using Org.BouncyCastle.Tls;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public class TcpXmppBackend(bool forceTls) : IXmppClientBackend
{
  private IXmppAddressProvider Provider { get; } = new XmppAddressProvider();
  
  private TcpClient? Client { get; set; }
  private NetworkStream? Stream { get; set; }
  private SslStream? SslStream { get; set; }
  private XmppAddress? Address { get; set; }
  
  private async Task UpgradeSslStream(IXmppClient xmppClient)
  {
    if (Client is null || Stream is null)
    {
      Console.WriteLine("Upgrade to SSL failed - no active TCP Stream");
      return;
    }

    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs() {Stream = null});
    await xmppClient.StopBackgroundService();
    
    // SslStream = new SslStream(Stream, false);
    // await SslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
    // {
    //   AllowRenegotiation = false,
    //   TargetHost = Address!.Host.TrimEnd(".").ToString()
    // });

    var target = Address!.Host.TrimEnd(".").ToString();
    var protocol = new TlsClientProtocol(Stream);
    var client = new XmppTlsClient(target);
    protocol.Connect(client);
    
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs() {Stream = protocol.Stream});
    xmppClient.StartBackgroundService();
    xmppClient.ReadLock.Release();
  }

  private async Task OnStartTlsProceed(object sender, object? stanza)
  {
    Console.WriteLine("Server confirmed TLS upgrade, proceeding...");
    await UpgradeSslStream((IXmppClient) sender);
      
    await ((XmppClient3)sender).OpenXmppStream();
    Console.WriteLine("TLS upgrade complete");
  }

  private Task OnStartTlsFailure(object sender, object? stanza)
  {
    var client = (IXmppClient) sender;
    Console.WriteLine("Server rejected TLS upgrade");
    client.InvokeClientError(new StartTls.Failure());
    return Task.CompletedTask;
  }

  public void UseClient(IXmppClient client)
  {
    client.RegisterUnexpectedStanza<StartTls.Proceed>(OnStartTlsProceed);
    client.RegisterUnexpectedStanza<StartTls.Failure>(OnStartTlsFailure);
  }

  public async void OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs)
  {
    var client = (IXmppClient) sender!;
    if (eventArgs.Feature is Features.StartTlsFeature || (SslStream is null && forceTls))
    {
      Console.WriteLine("Attempting to upgrade session to TLS");
      await client.SendStanzaAsync(new StartTls.Command());
    }
  }

  public event EventHandler<NetworkStreamUpdatedEventArgs>? NetworkStreamUpdated;

  public void Dispose()
  {
    Stream?.Dispose();
    Client?.Dispose();
    SslStream?.Dispose();
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
    Address = address;
    
    await Client.ConnectAsync(address.Ip, address.Port);
    
    Stream = Client.GetStream();
    Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
    
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs { Stream = Stream });
    return Result.Ok();
  }

  public void Disconnect()
  {
    SslStream?.Close();
    Stream?.Close();
    Client?.Close();
    
    SslStream = null;
    Stream = null;
    Client = null;
    
    NetworkStreamUpdated?.Invoke(this, new  NetworkStreamUpdatedEventArgs { Stream = null });
  }
}
