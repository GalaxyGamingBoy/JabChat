using System.CommandLine;
using Spectre.Console;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class StateCommand : Command
{
  private readonly IXmppClient _client;
  
  public StateCommand(IXmppClient client) : base("state", "Query's the XMPP Client State")
  {
    _client = client;
    
    SetAction(CommandAction);
  }

  private void CommandAction(ParseResult result)
  {
    AnsiConsole.MarkupLine($"XMPP Client State: [yellow]{_client.State}[/]");
  }
}