using System.CommandLine;
using OneOf;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Errors;
using XMPP.Core.IM;

namespace Cli.Commands.Repl.Presence;

using SendPresenceResult = OneOf<
  Unit,
  SendPresenceResults.SerializationFailure,
  SendPresenceResults.WriterNullException
>;

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

    SendPresenceResult result;
    if (directedTo is null)
      result = await im.SendOfflinePresence(reason);
    else
      result = await im.SendDirectedOfflinePresence(directedTo, reason);
    
    if (result.IsT0) return;
    var err = (IClientError) result.Value;
    AnsiConsole.MarkupLine($"[bold red]An error occured while preapproving the presence subscription: {err.What()}[/]");
  }
}