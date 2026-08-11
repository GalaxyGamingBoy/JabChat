using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Roster;

public class ViewCommand : Command
{
  private readonly IXmppClient _client;
  
  public ViewCommand(IXmppClient client) : base("view", "View the current client roster")
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
    
    AnsiConsole.MarkupLine($"Cached Roster Version: [yellow]{im.CachedVersion}[/]");
    AnsiConsole.MarkupLine("Roster Items:");
    foreach (var item in im.RosterItems)
    {
      var groups = string.Join(", ", item.Groups);
      AnsiConsole.MarkupLine($"* [blue]{item.Jid}[/] ({item.Name}) in groups \"{groups}\"");
    }
  }
}