using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Roaster;

public class GetCommand : Command
{
  private readonly IXmppClient _client;
  
  public GetCommand(IXmppClient client) : base("get", "Get the current roaster of the connected jid")
  { 
    _client = client; 
    
    SetAction(CommandAction);
  }
  
  private async Task CommandAction(ParseResult result)
  {
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    { 
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]"); 
      return;
    }

    await im.GetRoaster();
  }
}