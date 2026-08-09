using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Roster;

public class DeleteCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _jid = new("jid")
  {
    Description = "JID of the roster item to delete",
    Arity = ArgumentArity.ExactlyOne
  };
  
  public DeleteCommand(IXmppClient client) : base("delete", "Delete a roster item")
  {
    _client = client;
    
    Add(_jid);
    SetAction(CommandAction);
  }

  private async Task CommandAction(ParseResult parseResult)
  {
    var jid = parseResult.GetRequiredValue(_jid);

    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }
    
    var result = await im.DeleteRosterItem(jid);
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while getting the roster: {err.What()}[/]");

  }
}