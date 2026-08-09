using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Roster;

public class UpsertCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _jid = new("jid")
  {
    Description = "Roster item JID",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Option<string> _name = new("name")
  {
    Description = "Roster item name",
    Arity = ArgumentArity.ZeroOrOne
  };

  private readonly Option<string[]> _groups = new("groups")
  {
    Description = "Roster item groups",
    Arity = ArgumentArity.ZeroOrMore
  };
  
  public UpsertCommand(IXmppClient client) : base("upsert", "Add or update a roster item")
  {
    _client = client;
    
    Add(_jid);
    Add(_name);
    Add(_groups);
    SetAction(CommandAction);
  }

  private async Task CommandAction(ParseResult parseResult)
  {
    var jid = parseResult.GetRequiredValue(_jid);
    var name = parseResult.GetValue(_name);
    var groups = parseResult.GetValue(_groups);
    
    var item = new RosterItem() {Jid = jid, Name = name, Groups = groups?.ToList() ?? []};
    
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }
    
    var result = await im.UpsertRosterItem(item);
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while upserting the roster: {err.What()}[/]");
  }
}