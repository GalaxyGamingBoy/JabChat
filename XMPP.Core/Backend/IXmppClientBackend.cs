using FluentResults;
using Org.BouncyCastle.Tls;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public interface IXmppClientBackend : IDisposable
{
  Task<Result> ConnectAsync(XmppAddress address);
  
  void Disconnect();
  
  void UseClient(IXmppClient client);
  
  void OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs);
  
  event EventHandler<NetworkStreamUpdatedEventArgs> NetworkStreamUpdated; 
  
  ProtocolVersion? ClientProtocolVersion { get; }
  byte[] GetChannelBindingData();
}