using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence;

public class OfflineCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Option<string> _reason = new("--reason")
  {
    Description = "The reason for the offline presence update",
    Arity = ArgumentArity.ZeroOrOne
  };

  private readonly Option<string> _directedTo = new("--directed-to")
  {
    Description = "The directed jid to the offline presence update",
    Arity = ArgumentArity.ZeroOrOne
  };
  
  public OfflineCommand(IXmppClient client) : base("offline", "Set the current client presence to offline")
  {
    _client = client;
    
    Add(_reason);
    Add(_directedTo);
    SetAction(CommandAction);
  }

  private async Task CommandAction(ParseResult parseResult)
  {
    var reason = parseResult.GetValue(_reason);
    var directedTo = parseResult.GetValue(_directedTo);
    
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }

    if (directedTo is null)
      await im.SendOfflinePresence(reason);
    else
      await im.SendDirectedOfflinePresence(directedTo, reason);
  }
}