using System.Net.Sockets;
using FluentResults;
using XMPP.Core.Address;

namespace XMPP.Core.Backend;

public interface IXmppClientBackend : IDisposable
{
  Task<Result> ConnectAsync(string host);
  Task<Result> ConnectAsync(XmppAddress address);
  
  void Disconnect();
  
  Task OnStreamFeatureRequested(object? sender, StreamFeatureRequestedEventArgs eventArgs);
  Task OnUnexpectedStanzaReceived(object? sender, UnexpectedStanzaReceivedEventArgs eventArgs);
  
  event EventHandler<NetworkStreamUpdatedEventArgs> NetworkStreamUpdated;  
}