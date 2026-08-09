using System.Diagnostics;
using OneOf;
using XMPP.Core.Errors;
using XMPP.Core.EventArgs;
using XMPP.Core.InfoQueries;
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

public class ImExtension : IXmppClientExtension<ImExtension>
{
  public static int ExtensionIdentifier => 0;

  public static XmppClientExtensionLoadAt LoadAt => XmppClientExtensionLoadAt.AndActivateOnSuccess;

  public static ImExtension Create(IXmppClient client) => new(client);
  
  public Task LoadAsync()
  {
    return Task.CompletedTask;
  }

  public Task ActivateAsync()
  {
    return Task.CompletedTask;
  }

  public List<RosterItem> RosterItems = [];
  
  private readonly IXmppClient _client;

  static ImExtension()
  {
    XmppClientRegistry.RegisterInfoQuery<InfoQueryRoster>();
  }

  public ImExtension(IXmppClient client)
  {
    _client = client;
    _client.OnUnexpectedInfoQueryReceived += OnUnexpectedInfoQueryReceived;
  }

  /// <summary>
  /// Get the latest roster of the connected JID from the server.
  /// Usually called only once, after a successful connection.
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-syntax-actions-get">
  /// RFC6121 - 2.1.3. Roster Get
  /// </seealso>
  public async Task<GetRosterResult> GetRoster()
  {
    var iq = new InfoQuery(type: InfoQueryType.Get) { From =  _client.ConnectedJid.ToString() };
    iq.AddExtensionObject(new InfoQueryRoster());
    
    var result = await _client.SendInfoQueryAsync(iq);
    return result.Match<GetRosterResult>(
      iqr =>
      {
        var roster = iqr.GetExtensionObject<InfoQueryRoster>()!;
        RosterItems = roster.RosterItems;
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
        return infoQueryError.StanzaError.Errors[0] switch
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
        return infoQueryError.StanzaError.Errors[0] switch
        {
          ItemNotFound => new DeleteRosterItemResults.ItemNotFound(),
          _ => infoQueryError
        };
      },
      infoQueryError => infoQueryError,
      serializationFailure => serializationFailure);
  }

  /// <summary>
  /// Handle Roster Push Queries
  /// </summary>
  /// <seealso href="https://xmpp.org/rfcs/rfc6121.html#roster-syntax-actions-push">
  /// RFC6121 - 2.1.6. Roster Push
  /// </seealso>
  private void OnUnexpectedInfoQueryReceived(object? sender, OnUnexpectedInfoQueryReceivedEventArgs e)
  {
    var rosterPushIq = e.InfoQuery.GetExtensionObject<InfoQueryRoster>();
    if (rosterPushIq is null) return;

    if (e.InfoQuery.From is not null && e.InfoQuery.From != _client.ConnectedJid.BareJid)
      return;

    var serverItem = rosterPushIq.RosterItems.Single();
    var hasJid = RosterItems.Count(r => r.Jid == serverItem.Jid) == 1;
    if (hasJid)
      RosterItems.RemoveAll(r => r.Jid == serverItem.Jid);
    if (serverItem.Subscription != RosterItemSubscription.Remove)
      RosterItems.Add(serverItem);
    
    var iq = new InfoQuery(type: InfoQueryType.Result) { From = _client.ConnectedJid.ToString() };
    _ = Task.Run(async () => await _client.SendInfoQueryAsync(iq));
  }
  
  public ValueTask DisposeAsync()
  {
    _client.OnUnexpectedInfoQueryReceived -= OnUnexpectedInfoQueryReceived;
    return new ValueTask();
  }
}