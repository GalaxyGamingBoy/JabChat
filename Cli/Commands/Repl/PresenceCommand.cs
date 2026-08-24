using System.CommandLine;
using Cli.Commands.Repl.Presence;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class PresenceCommand : Command
{
  public PresenceCommand(IXmppClient client) : base("presence", "Manage XMPP presence")
  {
    Subcommands.Add(new InitialCommand(client));
    Subcommands.Add(new RequestCommand(client));
    Subcommands.Add(new UpdateCommand(client));
    Subcommands.Add(new OfflineCommand(client));
  }
}