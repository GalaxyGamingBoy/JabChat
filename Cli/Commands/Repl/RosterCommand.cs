using System.CommandLine;
using Cli.Commands.Repl.Roster;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class RosterCommand : Command
{
  public RosterCommand(IXmppClient client) : base("roster", "Manage the user XMPP roaster")
  {
    Subcommands.Add(new GetCommand(client));
    Subcommands.Add(new ViewCommand(client));
    Subcommands.Add(new UpsertCommand(client));
    Subcommands.Add(new DeleteCommand(client));
  }
}