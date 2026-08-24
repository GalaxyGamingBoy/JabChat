using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence;

public class UpdateCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _status = new("status")
  {
    Description = "Status message",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Option<PresenceShow> _show = new("--show")
  {
    Description = "Show status",
    Arity = ArgumentArity.ZeroOrOne,
    DefaultValueFactory = _ => PresenceShow.Active
  };

  private readonly Option<int> _priority = new("--priority")
  {
    Description = "Priority",
    Arity = ArgumentArity.ZeroOrOne,
    DefaultValueFactory = (_ => 0)
  };
  
  private readonly Option<string> _directedTo = new("--directed-to")
  {
    Description = "The directed jid to the offline presence update",
    Arity = ArgumentArity.ZeroOrOne
  };
  
  public UpdateCommand(IXmppClient client) : base("update", "Update the current presence information")
  {
    _client = client;

    Add(_status);
    Add(_show);
    Add(_priority);
    Add(_directedTo);
    SetAction(CommandAction);
  }

  private async Task CommandAction(ParseResult parseResult)
  {
    var status = parseResult.GetRequiredValue(_status);
    var show = parseResult.GetRequiredValue(_show);
    var priority = parseResult.GetRequiredValue(_priority);
    var directedTo = parseResult.GetValue(_directedTo);
    
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }

    if (directedTo is null)
      await im.SendPresenceUpdate(new PresenceUpdate(show, status, priority));
    else
      await im.SendDirectedPresenceUpdate(directedTo, new PresenceUpdate(show, status, priority));
  }
}