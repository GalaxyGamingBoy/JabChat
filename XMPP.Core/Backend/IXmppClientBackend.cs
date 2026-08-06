using OneOf;
using Org.BouncyCastle.Tls;
using XMPP.Core.Address;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;

namespace XMPP.Core.Backend;

public interface IXmppClientBackend : IDisposable
{
  #region Connection Handling
  /// <summary>
  /// Notifies when there was an internal stream update
  /// </summary>
  event EventHandler<NetworkStreamUpdatedEventArgs> NetworkStreamUpdated; 

  /// <summary>
  /// Connect the backend client to the host
  /// </summary>
  /// <param name="address">xmpp address to connect to</param>
  /// <returns>connection result</returns>
  Task<OneOf<
    Unit,
    BackendConnectResults.AddressPortInvalid,
    BackendConnectResults.ClientAlreadyConnected,
    BackendConnectResults.ConnectionFailure
  >> ConnectAsync(XmppAddress address);
  
  /// <summary>
  /// Disconnects the internal xmpp client
  /// </summary>
  void Disconnect(); 
  #endregion

  #region Setup
  /// <summary>
  /// Bind the backend to a XMPP client
  /// </summary>
  /// <param name="client">client to bind to</param>
  void UseClient(IXmppClient client);
  
  /// <summary>
  /// Notifies the backend when new stream features where received
  /// </summary>
  /// <param name="sender">event sender</param>
  /// <param name="eventArgs">stream features received</param>
  void OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs);
  #endregion

  #region SASL
  /// <summary>
  /// Internal client protocol version used, controls channel binding
  /// </summary>
  ProtocolVersion? ClientProtocolVersion { get; }
  
  /// <summary>
  /// Gets the channel binding data from the baclend
  /// </summary>
  /// <returns></returns>
  byte[] GetChannelBindingData();
  #endregion
}