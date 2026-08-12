using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence;

public class InitialCommand : Command
{
  private readonly IXmppClient _client;
  
  public InitialCommand(IXmppClient client) : base("initial", "Send an initial presence")
  {
    _client = client;
    
    SetAction(CommandHandler);
  }

  private async Task CommandHandler(ParseResult _)
  {
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }

    var result = await im.SendInitialPresence();
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while sending initial presence: {err.What()}[/]");
  }
}