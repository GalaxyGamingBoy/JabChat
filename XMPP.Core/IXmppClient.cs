using System.Xml.Serialization;
using FluentResults;
using XMPP.Core.Address;
using XMPP.Core.StreamErrors;

namespace XMPP.Core;

public interface IXmppClient
{
  /// <summary>
  /// Connects asynchronously to the XMPP Server
  /// </summary>
  public Task<Result> ConnectAsync();
  
  /// <summary>
  /// Disconnects from the XMPP Server
  /// </summary>
  public Result Disconnect();

  public Task<Result> DisconnectWithStreamCloseAsync();
  
  /// <summary>
  /// Reconnects asynchronously to the XMPP Server
  /// </summary>
  public Task<Result> ReconnectAsync();

  /// <summary>
  /// Registers a XMPP stream feature for deserialization
  /// </summary>
  /// <typeparam name="T">Feature Object</typeparam>
  public Result RegisterFeature<T>();

  public Result RegisterStreamError<T>() where T : IStreamError;
  
  event EventHandler<StreamFeatureRequestedEventArgs> StreamFeatureRequested;
  event EventHandler<StreamErrorEventArgs> StreamErrorRaised;
}