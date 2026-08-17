using System.Collections.Concurrent;
using System.Diagnostics;
using OneOf;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.InfoQueries;
using XMPP.Core.Messages;
using XMPP.Core.Presence;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.IM;

using GetRosterResult = OneOf<
  Unit,
  SendInfoQueryResults.InfoQueryError,
  SendInfoQueryResults.SerializationFailure,
  SendInfoQueryResults.WriterNullException
>;

using UpsertRosterItemResult = OneOf<
  Unit,
  UpsertRosterItemResults.DuplicateGroups,
  UpsertRosterItemResults.LengthLimit,
  SendInfoQueryResults.InfoQueryError,
  SendInfoQueryResults.SerializationFailure,
  SendInfoQueryResults.WriterNullException
>;

using DeleteRosterItemResult = OneOf<
  Unit,
  DeleteRosterItemResults.ItemNotFound,
  SendInfoQueryResults.InfoQueryError,
  SendInfoQueryResults.SerializationFailure,
  SendInfoQueryResults.WriterNullException
>;

using RequestPresenceSubscriptionResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using RequestPresenceUnsubscriptionResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using ApprovePresenceSubscriptionResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using CancelPresenceSubscriptionResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using PreApprovePresenceSubscriptionResult = OneOf<
  Unit,
  PreApprovePresenceSubscriptionResults.PreApprovalNotSupported,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>; 

using SendInitialPresenceResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

using SendMessageResult = OneOf<
  Unit,
  SendMessageResults.SerializationFailure,
  SendMessageResults.WriterNullException
>;

public class ImExtension : IXmppClientExtension<ImExtension>
{
  public static int ExtensionIdentifier => 0;

  public static XmppClientExtensionActivateOn ActivateOn => XmppClientExtensionActivateOn.SaslComplete;

  public static ImExtension Create(IXmppClient client) => new(client);
  
  public Task ActivateAsync()
  {
    return Task.CompletedTask;
  }

  private readonly Lock _rosterLock = new();
  private List<RosterItem> _rosterItems = [];

  public IReadOnlyList<RosterItem> RosterItems
  {
    get
    {
      lock (_rosterLock)
        return _rosterItems.ToList();
    }
  }

  public string CachedVersion { get; private set; } = string.Empty;
  
  private readonly ConcurrentDictionary<string, ImMessageCache> _messageCache = new();

  private bool _rosterVersioningEnabled;
  private bool _presencePreApprovalEnabled;
  
  private readonly IXmppClient _client;

  static ImExtension()
  {
    XmppClientRegistry.RegisterInfoQuery<InfoQueryRoster>();
    
    XmppClientRegistry.RegisterFeature<ImRosterVersioningFeature>();
    XmppClientRegistry.RegisterFeature<ImPresencePreApproval>();
  }

  private ImExtension(IXmppClient client)
  {
    _client = client;
    _client.OnUnexpectedInfoQueryReceived += ClientOnUnexpectedInfoQueryReceived;
    _client.StreamFeatureAdvertised += ClientOnStreamFeatureAdvertised;
    _client.OnMessageReceived += ClientOnMessageReceived;
  }

  private void ClientOnStreamFeatureAdvertised(object? sender, StreamFeatureRequestedEventArgs e)
  {
    if (e.Feature is ImRosterVersioningFeature)
      _rosterVersioningEnabled = true;
    if (e.Feature is ImPresencePreApproval)
      _presencePreApprovalEnabled = true;
  }

  /// <summary>
  /// Get the latest roster of the connected JID from the server.
  /// Usually called only once, after a successful connection.
  /// CachedVersion keeps track of the roster version, if communicated by the server.
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-syntax-actions-get">
  /// RFC6121 - 2.1.3. Roster Get
  /// </seealso>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-versioning">
  /// RFC6121 - 2.6. Roster Versioning
  /// </seealso>
  public async Task<GetRosterResult> GetRoster()
  {
    var iq = new InfoQuery(type: InfoQueryType.Get) { From =  _client.ConnectedJid.ToString() };
    iq.AddExtensionObject(new InfoQueryRoster { Version = _rosterVersioningEnabled ? CachedVersion : null });
    
    var result = await _client.SendInfoQueryAsync(iq);
    return result.Match<GetRosterResult>(
      iqr =>
      {
        var roster = iqr.GetExtensionObject<InfoQueryRoster>();
        if (roster is null) return new Unit();
        
        lock (_rosterLock)
          _rosterItems = roster.RosterItems;
        CachedVersion = roster.Version ?? string.Empty;
        
        return new Unit();
      },
      infoQueryError => infoQueryError,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException);
  }

  /// <summary>
  /// Add or modify an item in the roster of the connected JID.
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-add">
  /// RFC6121 - 2.3. Adding a Roster Item
  /// </seealso>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-update">
  /// RFC6121 - 2.4. Updating a Roster Item
  /// </seealso>
  public async Task<UpsertRosterItemResult> UpsertRosterItem(RosterItem item)
  {
    var iq = new InfoQuery(type: InfoQueryType.Set) { From = _client.ConnectedJid.ToString() };
    iq.AddExtensionObject(new InfoQueryRoster() { RosterItems = [item]});
    
    var result = await _client.SendInfoQueryAsync(iq);
    return result.Match<UpsertRosterItemResult>(
      _ => new Unit(),
      infoQueryError =>
      {
        var error = infoQueryError.StanzaError.Errors.FirstOrDefault() ?? new GenericError();
        return error switch
        {
          BadRequest => new UpsertRosterItemResults.DuplicateGroups(),
          NotAcceptable => new UpsertRosterItemResults.LengthLimit(),
          _ => infoQueryError
        };
      },
      serializationFailure => serializationFailure,
      writerNullException => writerNullException);
  }

  /// <summary>
  /// Delete a roster item of the connected JID.
  /// </summary>
  /// <param name="jid">The JID to remove from the roster</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-delete">
  /// RFC6121 - 2.5. Deleting a Roster Item
  /// </seealso>
  public async Task<DeleteRosterItemResult> DeleteRosterItem(string jid)
  {
    var result = await UpsertRosterItem(new RosterItem { Jid = jid, Subscription = RosterItemSubscription.Remove});
    return result.Match<DeleteRosterItemResult>(
      unit => unit,
      _ => throw new UnreachableException(),
      _ => throw new UnreachableException(),
      infoQueryError =>
      {
        var error = infoQueryError.StanzaError.Errors.FirstOrDefault() ?? new GenericError();
        return error switch
        {
          ItemNotFound => new DeleteRosterItemResults.ItemNotFound(),
          _ => infoQueryError
        };
      },
      serializationFailure => serializationFailure,
      writerNullException => writerNullException);
  }

  /// <summary>
  /// Request a presence subscription to a JID.
  /// </summary>
  /// <param name="jid">The bare JID of the entity to subscribe to</param>
  /// <param name="reason">The reason why a presence subscription was requested</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-request">
  /// RFC6121 - 3.1. Requesting a Subscription
  /// </seealso>
  public async Task<RequestPresenceSubscriptionResult> RequestPresenceSubscription(string jid, string? reason = null)
  {
    List<string>? status = reason is null ? null : [reason];
    
    var presence = new Presence.Presence() { To = jid, Type = PresenceType.Subscribe, Status = status };
    return (await _client.SendPresenceAsync(presence)).Match<RequestPresenceSubscriptionResult>(
      unit => unit,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException
      );
  }
  
  /// <summary>
  /// Request an unsubscription of presence updates of a JID
  /// </summary>
  /// <param name="jid">The bare JID of the entity to subscribe to</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-unsub">
  /// RFC6121 - 3.3. Unsubscribing
  /// </seealso>
  public async Task<RequestPresenceUnsubscriptionResult> RequestPresenceUnsubscription(string jid)
  {
    var presence = new Presence.Presence() { To = jid, Type = PresenceType.Unsubscribe };
    return (await _client.SendPresenceAsync(presence)).Match<RequestPresenceUnsubscriptionResult>(
      unit => unit,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException
    );
  }

  /// <summary>
  /// Approve a presence subscription to a JID.
  /// </summary>
  /// <param name="jid">The bare JID of the entity to accept</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-request-handle">
  /// RFC6121 - 3.1.4. Client Processing of Inbound Subscription Request
  /// </seealso>
  public async Task<ApprovePresenceSubscriptionResult> ApprovePresenceSubscription(string jid)
  {
    var presence = new Presence.Presence() { To = jid, Type = PresenceType.Subscribed };
    return (await _client.SendPresenceAsync(presence)).Match<ApprovePresenceSubscriptionResult>(
      unit => unit,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException
      );
  }
  
  /// <summary>
  /// Cancel a presence subscription to a JID.
  /// </summary>
  /// <param name="jid">The bare JID of the entity to deny</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-request-handle">
  /// RFC6121 - 3.1.4. Client Processing of Inbound Subscription Request
  /// </seealso>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-cancel">
  /// RFC6121 - 3.2. Canceling a Subscription
  /// </seealso>
  public async Task<CancelPresenceSubscriptionResult> CancelPresenceSubscription(string jid)
  {
    var presence = new Presence.Presence() { To = jid, Type = PresenceType.Unsubscribed };
    return (await _client.SendPresenceAsync(presence)).Match<CancelPresenceSubscriptionResult>(
      unit => unit,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException
    );
  }

  /// <summary>
  /// Preapprove a presence subscription to a JID.
  /// </summary>
  /// <param name="jid">The bare JID of the entity to preapprove</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#sub-preapproval">
  /// RFC6121 - 3.4. Pre-Approving a Subscription Request
  /// </seealso>
  public async Task<PreApprovePresenceSubscriptionResult> PreApprovePresenceSubscriptionResult(string jid)
  {
    if (!_presencePreApprovalEnabled)
      return new PreApprovePresenceSubscriptionResults.PreApprovalNotSupported();
    
    return (await ApprovePresenceSubscription(jid)).Match<PreApprovePresenceSubscriptionResult>(
      unit => unit,
      serializationFailure => serializationFailure,
      writerNullException => writerNullException
      );
  }

  /// <summary>
  /// Notify the server of the clients initial online presence
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-initial">
  /// RFC6121 - 4.2. Initial Presence
  /// </seealso>
  public async Task<SendInitialPresenceResult> SendInitialPresence()
  {
    var iq = new Presence.Presence() {Type = PresenceType.None};
    return await _client.SendPresenceAsync(iq);
  }

  /// <summary>
  /// Send a presence update to subscribed entities
  /// </summary>
  /// <param name="update">Presence update to send</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-broadcast">
  /// RFC6121 - 4.4. Subsequent Presence Broadcast
  /// </seealso>
  public async Task SendPresenceUpdate(PresenceUpdate update)
  {
    List<string>? status = update.Status is null ? null : [update.Status];
    var presence = new Presence.Presence() {
      Type = PresenceType.None,
      Show = update.Show,
      Status = status,
      Priority =  update.Priority,
    };
    
    await _client.SendPresenceAsync(presence);
  }
  /// <summary>
  /// Send a presence update directly to an entity
  /// </summary>
  /// <param name="jid">JID to send the presence update tp</param>
  /// <param name="update">Presence update to send</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-broadcast">
  /// RFC6121 - 4.4. Subsequent Presence Broadcast
  /// </seealso>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-directed">
  /// RFC6121 - 4.6. Directed Presence
  /// </seealso>
  public async Task SendDirectedPresenceUpdate(string jid, PresenceUpdate update)
  {
    List<string>? status = update.Status is null ? null : [update.Status];
    var presence = new Presence.Presence() {
      Type = PresenceType.None,
      Show = update.Show,
      Status = status,
      Priority =  update.Priority,
      To = jid,
    };
    
    await _client.SendPresenceAsync(presence);
  }

  /// <summary>
  /// Send a presence update notified subscribed entities that it will go offline
  /// </summary>
  /// <param name="reason">The reason the entity will go offline</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-unavailable">
  /// RFC6121 - 4.5. Unavailable Presence
  /// </seealso>
  public async Task SendOfflinePresence(string? reason)
  {
    List<string>? status = reason is null ? null : [reason];
    var presence = new Presence.Presence()
    {
      Type = PresenceType.Unavailable,
      Status = status
    };
    
    await _client.SendPresenceAsync(presence);
  }
  
  /// <summary>
  /// Send a presence update notifying the specified entity that it will go offline
  /// </summary>
  /// <param name="jid">JID to send the presence update tp</param>
  /// <param name="reason">The reason the entity will go offline</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-unavailable">
  /// RFC6121 - 4.5. Unavailable Presence
  /// </seealso>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#presence-directed">
  /// RFC6121 - 4.6. Directed Presence
  /// </seealso>
  public async Task SendDirectedOfflinePresence(string jid, string? reason)
  {
    List<string>? status = reason is null ? null : [reason];
    var presence = new Presence.Presence()
    {
      Type = PresenceType.Unavailable,
      Status = status,
      To = jid,
    };
    
    await _client.SendPresenceAsync(presence);
  }

  /// <summary>
  /// Send a message to a user
  /// </summary>
  /// <param name="message">Message contents</param>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#message">
  /// RFC6121 - 5. Exchanging Messages
  /// </seealso>
  public async Task<SendMessageResult> SendMessage(ImMessage message)
  {
    var key = $"{nameof(MessageType.Chat)};{message.ToBare}";
    _messageCache.TryGetValue(key, out var cached);
    
    // XEP0201 - If an entity receives a message of type "chat" without a thread ID,
    // then it SHOULD create a new session with a new thread ID (and include that thread ID
    // in all the messages it sends within the new session)
    // href: https://xmpp.org/extensions/xep-0201.html#chat
    var thread = cached?.Thread ?? new MessageThread { Body = Guid.NewGuid().ToString() };
    var to = cached?.FromFullJid ?? message.ToBare;
    
    var msg = new Message
    {
      To = to,
      From = _client.ConnectedJid.ToString(),
      Thread = thread,
      Type = MessageType.Chat
    };

    return await _client.SendMessageAsync(msg);
  }

  /// <summary>
  /// Handle Roster Push Queries
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-syntax-actions-push">
  /// RFC6121 - 2.1.6. Roster Push
  /// </seealso>
  private void ClientOnUnexpectedInfoQueryReceived(object? sender, OnUnexpectedInfoQueryReceivedEventArgs e)
  {
    var rosterPushIq = e.InfoQuery.GetExtensionObject<InfoQueryRoster>();
    if (rosterPushIq is null) return;

    if (e.InfoQuery.From is not null && e.InfoQuery.From != _client.ConnectedJid.BareJid)
      return;

    if (_rosterVersioningEnabled)
      CachedVersion = rosterPushIq.Version ?? string.Empty;
    
    if (rosterPushIq.RosterItems.Count != 1)
      return;
    
    lock (_rosterLock) {
      var serverItem = rosterPushIq.RosterItems.Single();
      _rosterItems.RemoveAll(r => r.Jid == serverItem.Jid);
      if (serverItem.Subscription != RosterItemSubscription.Remove)
        _rosterItems.Add(serverItem);
    }
    
    var iq = new InfoQuery(type: InfoQueryType.Result) { From = _client.ConnectedJid.ToString(), Id = e.InfoQuery.Id };
    _ = Task.Run(async () => await _client.SendInfoQueryAsync(iq));
  }

  /// <summary>
  /// Handle message thread and jid updates
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#message-syntax-thread">
  /// RFC6121 - 5.2.5. Thread Element
  /// </seealso>
  private void ClientOnMessageReceived(object? sender, OnMessageReceivedEventArgs e)
  {
    if (e.Message.Thread is null) return;
    
    var bareFrom = e.Message.From.Split("/")[0];
    var key = $"{e.Message.Type.ToString()};{bareFrom}";
    _messageCache[key] = new ImMessageCache(bareFrom, e.Message.Thread);
  }
  
  public ValueTask DisposeAsync()
  {
    _client.OnUnexpectedInfoQueryReceived -= ClientOnUnexpectedInfoQueryReceived;
    _client.StreamFeatureAdvertised -= ClientOnStreamFeatureAdvertised;
    _client.OnMessageReceived -= ClientOnMessageReceived;
    
    return new ValueTask();
  }
}