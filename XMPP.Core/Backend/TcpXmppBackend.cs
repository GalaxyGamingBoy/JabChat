using System.Net.Sockets;
using OneOf;
using Org.BouncyCastle.Tls;
using XMPP.Core.Address;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.StartTls;

namespace XMPP.Core.Backend;

public class TcpXmppBackend(bool forceTls) : IXmppClientBackend
{
  #region Internal Fields
  private TcpClient? Client { get; set; }
  private XmppTlsClient? TlsClient { get; set; }
  private NetworkStream? Stream { get; set; }
  
  private string ConnectedHost { get; set; } = string.Empty; 
  #endregion
  
  
  #region Setup
  public void UseClient(IXmppClient client)
  {
    client.RegisterUnexpectedStanza<Proceed>(OnStartTlsProceed);
    client.RegisterUnexpectedStanza<Failure>(OnStartTlsFailure);
  }
  
  // ReSharper disable once AsyncVoidEventHandlerMethod - XmppClient methods protected by result
  public async void OnStreamFeatureAdvertised(object? sender, StreamFeatureRequestedEventArgs eventArgs)
  {
    var client = (IXmppClient) sender!;
    if (eventArgs.Feature is Features.StartTlsFeature || (TlsClient is null && forceTls))
    {
      Console.WriteLine("Attempting to upgrade session to TLS");
      await client.SendStanzaAsync(new Command());
    }
  }
  
  public void Dispose()
  {
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs { Stream = null });
    
    Stream?.Dispose();
    Stream = null;
    
    Client?.Dispose();
    Client = null;
    
    GC.SuppressFinalize(this);
  }
  #endregion

  
  #region Connection Handling
  public event EventHandler<NetworkStreamUpdatedEventArgs>? NetworkStreamUpdated;
  
  public async Task<OneOf<
    Unit,
    BackendConnectResults.AddressPortInvalid,
    BackendConnectResults.ClientAlreadyConnected,
    BackendConnectResults.ConnectionFailure
  >> ConnectAsync(XmppAddress address)
  {
    try
    {
      if (Client is not null || Stream is not null)
        return new BackendConnectResults.ClientAlreadyConnected();

      Client = new TcpClient();
      ConnectedHost = address.Host.TrimEnd(".").ToString();

      Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
      await Client.ConnectAsync(address.Ip, address.Port);
      Stream = Client.GetStream();

      NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs { Stream = Stream });

      return new Unit();
    }
    catch (ArgumentOutOfRangeException)
    {
      Dispose();
      return new BackendConnectResults.AddressPortInvalid();
    }
    catch (SocketException)
    {
      Dispose();
      return new BackendConnectResults.ConnectionFailure();
    }
  }

  public void Disconnect() => Dispose(); 
#endregion
  
  
  #region Ssl Upgrade Logic
  private async Task UpgradeSslStream(IXmppClient xmppClient)
  {
    if (Client is null || Stream is null)
    {
      Console.WriteLine("Upgrade to SSL failed - no active TCP Stream");
      return;
    }

    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs() {Stream = null});
    await xmppClient.StopBackgroundService();

    var protocol = new TlsClientProtocol(Stream);
    TlsClient = new XmppTlsClient(ConnectedHost);
    
    try
    {
      protocol.Connect(TlsClient);
    }
    catch (IOException)
    {
      xmppClient.InvokeClientError(new Failure());
    }
    
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs() {Stream = protocol.Stream});
    xmppClient.StartBackgroundService();
    xmppClient.ReadLock.Release();
  } 
  
  private async Task OnStartTlsProceed(object sender, object? stanza)
  {
    Console.WriteLine("Server confirmed TLS upgrade, proceeding...");
    await UpgradeSslStream((IXmppClient) sender);
      
    await ((XmppClient)sender).OpenXmppStream();
    Console.WriteLine("TLS upgrade complete");
  }
  
  private Task OnStartTlsFailure(object sender, object? stanza)
  {
    var client = (IXmppClient) sender;
    Console.WriteLine("Server rejected TLS upgrade");
    client.InvokeClientError(new Failure());
    return Task.CompletedTask;
  }
  #endregion


  #region SASL
  public ProtocolVersion? ClientProtocolVersion => TlsClient?.GetNegotiatedVersion();
  
  public byte[] GetChannelBindingData()
  {
    return TlsClient?.GetChannelBindingData() ?? [];
  }
  #endregion
}
