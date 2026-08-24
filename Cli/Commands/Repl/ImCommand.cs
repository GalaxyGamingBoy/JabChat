using System.CommandLine;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class ImCommand : Command
{
  public ImCommand(IXmppClient client) : base("im", "Check IM extension status")
  {
    Subcommands.Add(new Im.RosterVersioningCommand(client));
    Subcommands.Add(new Im.PresencePreApproval(client));
  }
}