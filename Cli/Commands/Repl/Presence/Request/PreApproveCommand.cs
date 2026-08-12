using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence.Request;

public class PreApproveCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _jid = new("jid")
  {
    Description = "The JID to preapprove a presence request from",
    Arity = ArgumentArity.ExactlyOne
  };

  public PreApproveCommand(IXmppClient client) : base("preapprove", "Preapprove a presence request from a JID")
  {
    _client = client;
    
    Add(_jid);
    SetAction(CommandHandler);
  }

  private async Task CommandHandler(ParseResult parseResult)
  {
    var jid = parseResult.GetRequiredValue(_jid);

    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }

    var result = await im.PreApprovePresenceSubscriptionResult(jid);
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while preapproving the presence subscription: {err.What()}[/]");
  }
}