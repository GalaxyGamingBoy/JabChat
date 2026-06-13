using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;

namespace XMPP.Core;

public interface IXmppClient
{
  // new api
  
  /// <summary>
  /// Connects asynchronously to the XMPP Server
  /// </summary>
  public Task<Result> ConnectAsync();
  
  /// <summary>
  /// Disconnects from the XMPP Server
  /// </summary>
  public Task<Result> DisconnectAsync();
  
  /// <summary>
  /// Reconnects asynchronously to the XMPP Server
  /// </summary>
  public Task<Result> ReconnectAsync();

  /// <summary>
  /// Registers a XMPP stream feature for deserialization
  /// </summary>
  /// <typeparam name="T">Feature Object</typeparam>
  public Result RegisterFeature<T>();
}