using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using OneOf;
using Org.BouncyCastle.Tls;
using XMPP.Core.Address;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.LogMessages;
using XMPP.Core.StartTls;
using TcpXmppBackendLogs = XMPP.Core.LogMessages.TcpXmppBackendLogs;

namespace XMPP.Core.Backend;

public class TcpXmppBackend(bool forceTls) : IXmppClientBackend
{
  #region Internal Fields
  private TcpClient? Client { get; set; }
  private XmppTlsClient? TlsClient { get; set; }
  private NetworkStream? Stream { get; set; }
  
  private string ConnectedHost { get; set; } = string.Empty; 
  
  private ILogger<TcpXmppBackend> _logger = JabChatLogging.Factory.CreateLogger<TcpXmppBackend>();
  #endregion
  
  
  #region Setup
  public void UseClient(IXmppClient client)
  {
    TcpXmppBackendLogs.BindToClient(_logger);
    client.RegisterUnexpectedStanza<Proceed>(OnStartTlsProceed);
    client.RegisterUnexpectedStanza<Failure>(OnStartTlsFailure);
  }
  
  // ReSharper disable once AsyncVoidEventHandlerMethod - XmppClient methods protected by result
  public async void OnStreamFeatureAdvertised(object? sender, StreamFeatureRequestedEventArgs eventArgs)
  {
    var client = (IXmppClient) sender!;
    if (eventArgs.Feature is Features.StartTlsFeature || (TlsClient is null && forceTls))
    {
      TcpXmppBackendLogs.StartTlsUpgradeRequest(_logger);
      // Console.WriteLine("Attempting to upgrade session to TLS");
      await client.SendStanzaAsync(new Command());
    }
  }
  
  public void Dispose()
  {
    TcpXmppBackendLogs.DisposingBackend(_logger);
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
      
      TcpXmppBackendLogs.Connect(_logger, address.Ip, address.Port);

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
      TcpXmppBackendLogs.TlsUpgradeFailedNoTcp(_logger);
      return;
    }
    
    NetworkStreamUpdated?.Invoke(this, new NetworkStreamUpdatedEventArgs() {Stream = null});
    await xmppClient.StopBackgroundService();
    
    TcpXmppBackendLogs.UpgradingStreamToTls(_logger);

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
    TcpXmppBackendLogs.ServerConfirmedTlsUpgrade(_logger);
    await UpgradeSslStream((IXmppClient) sender);
      
    await ((XmppClient)sender).OpenXmppStream();
    TcpXmppBackendLogs.StreamUpgradedToTls(_logger);
  }
  
  private Task OnStartTlsFailure(object sender, object? stanza)
  {
    TcpXmppBackendLogs.ServerRejectedTlsUpgrade(_logger);
    var client = (IXmppClient) sender;
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
