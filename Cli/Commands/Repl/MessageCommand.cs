using System.CommandLine;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.IM;

namespace Cli.Commands.Repl;

public class MessageCommand : Command
{
  private readonly IXmppClient _client;

  private readonly Argument<string> _to = new("to")
  {
    Description = "The JID to send the message to",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _message = new("message")
  {
    Description = "The message to sent",
    Arity = ArgumentArity.ExactlyOne
  };
    
  public MessageCommand(IXmppClient client) : base("message", "Send a message to a JID")
  {
    _client = client;

    Add(_to);
    Add(_message);
    SetAction(CommandAction);
  }

  private async Task CommandAction(ParseResult parseResult)
  {
    var to = parseResult.GetRequiredValue(_to);
    var message = parseResult.GetRequiredValue(_message);
    
    var im = _client.GetExtension<ImExtension>();
    if (im is null)
    {
      AnsiConsole.MarkupLine("[bold red]IM extension is not enabled yet[/]");
      return;
    }

    await im.SendMessage(new ImMessage(to, message));
  }
}