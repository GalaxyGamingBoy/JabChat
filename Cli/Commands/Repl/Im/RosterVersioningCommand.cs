using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Im;

public class RosterVersioningCommand : Command
{
  private readonly IXmppClient _client;
  
  public RosterVersioningCommand(IXmppClient client) : base("roster-versioning", "Check if Roster Versioning is enabled from the server")
  {
    _client = client;
    SetAction(CommandAction);
  }

  private void CommandAction(ParseResult _)
  {
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    } 
    
    var s = im.RosterVersioningEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]";
    AnsiConsole.MarkupLine($"[yellow]Roster Versioning[/]: {s}");
  }
}