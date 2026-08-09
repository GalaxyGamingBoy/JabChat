using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Roster;

public class GetCommand : Command
{
  private readonly IXmppClient _client;
  
  public GetCommand(IXmppClient client) : base("get", "Get the current roster of the connected jid from the server")
  { 
    _client = client; 
    
    SetAction(CommandAction);
  }
  
  private async Task CommandAction(ParseResult _)
  {
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    { 
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]"); 
      return;
    }

    var result = await im.GetRoster();
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while getting the roster: {err.What()}[/]");
  }
}