using System.Xml;
using System.Xml.Linq;
using FluentResults;
using XMPP.Core.InfoQueries;
using XMPP.Core.SaslMechanisms;
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
  public Task<Result> Disconnect();

  public Task<Result> DisconnectWithStreamCloseAsync();
  
  /// <summary>
  /// Reconnects asynchronously to the XMPP Server
  /// </summary>
  public Task<Result> ReconnectAsync();

  public Task<Result> SendStanzaAsync(object element);
  public Task<Result> SendStanzaAsync(XElement element);
  
  public Task<Result<InfoQuery>> SendInfoQueryAsync(InfoQuery query);

  /// <summary>
  /// Registers a XMPP stream feature for deserialization
  /// </summary>
  /// <typeparam name="T">Feature Object</typeparam>
  public Result RegisterFeature<T>();

  public Result RegisterUnexpectedStanza<T>(Func<object, object?, Task> func);


  public Result RegisterClientError<T>() where T : IClientError;

  public void RegisterSaslMechanism<T>() where T : ISaslMechanism, new();

  public Task<Result> OpenXmppStream();
  
  event EventHandler<StreamFeatureRequestedEventArgs>? StreamFeatureRequestedAsync;
  event EventHandler<StreamErrorEventArgs>? ClientErrorRaisedAsync;
  
  public Task SaslCompleted();

  public void StartBackgroundService();
  public Task StopBackgroundService();

  public void InvokeClientError(IClientError error);
  
  /// <summary>
  /// Manages the ReadLock of the backend service, usually there is no need to touch (exceptions exist)
  ///
  /// The only time you are supposed to release the ReadLock is when:
  ///   + You receive an unexpected stanza, the readlock is transferred to you
  /// </summary>
  public SemaphoreSlim ReadLock { get; }
}