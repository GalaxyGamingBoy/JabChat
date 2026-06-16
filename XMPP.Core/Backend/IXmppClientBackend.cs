using System.Net.Sockets;
using FluentResults;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public interface IXmppClientBackend : IDisposable
{
  Task<Result> ConnectAsync(string host);
  Task<Result> ConnectAsync(XmppAddress address);
  
  void Disconnect();
  
  void OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs);
  
  event EventHandler<NetworkStreamUpdatedEventArgs> NetworkStreamUpdated;  
}