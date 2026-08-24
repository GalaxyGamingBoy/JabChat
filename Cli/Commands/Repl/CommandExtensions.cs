using System.CommandLine;
using XMPP.Core;

namespace Cli.Commands.Repl;

public static class CommandExtensions
{
  public static void AddReplCommands(this RootCommand root, IXmppClient client)
  {
    root.Subcommands.Add(new StateCommand(client));
    root.Subcommands.Add(new ReadLockCommand(client));
    root.Subcommands.Add(new RosterCommand(client));
    root.Subcommands.Add(new PresenceCommand(client));
    root.Subcommands.Add(new ImCommand(client));
    root.Subcommands.Add(new MessageCommand(client));
  }
}