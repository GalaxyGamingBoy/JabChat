using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence.Request;

public class UnsubscribeCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _jid = new("jid")
  {
    Description = "The JID to unsubscribe from",
    Arity = ArgumentArity.ExactlyOne
  };

  public UnsubscribeCommand(IXmppClient client) : base("unsubscribe", "Unsubscribe to a presence subscription from a JID")
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

    var result = await im.RequestPresenceUnsubscription(jid);
    if (result.IsT0) return;
    
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while unsubscribing: {err.What()}[/]");
  }
}