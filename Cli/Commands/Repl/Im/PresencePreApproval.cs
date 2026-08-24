using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Im;

public class PresencePreApproval : Command
{
  private readonly IXmppClient _client;
  
  public PresencePreApproval(IXmppClient client) : base("presence-preapproval", "Check if Presence PreApproval is enabled from the server")
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
    
    var s = im.PresencePreApprovalEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]";
    AnsiConsole.MarkupLine($"[yellow]Presence PreApproval[/]: {s}");
  }
}