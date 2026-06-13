using System.Net.Sockets;
using FluentResults;

namespace XMPP.Core.Backend;

public interface IXmppClientBackend : IDisposable
{
  Task<Result> ConnectAsync(string host);
  void Disconnect();
  
  NetworkStream? Stream { get; }
}