using System.CommandLine;
using Spectre.Console;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class ReadLockCommand : Command
{
  private readonly IXmppClient _client;
  
  public ReadLockCommand(IXmppClient client) : base("readlock", "Query's the XMPP Client ReadLock status")
  {
    _client = client;
    
    SetAction(CommandAction);
  }

  private void CommandAction(ParseResult result)
  {
    AnsiConsole.MarkupLine($"XMPP ReadLock: [yellow]{_client.ReadLock.CurrentCount}/1[/]");
  }
}