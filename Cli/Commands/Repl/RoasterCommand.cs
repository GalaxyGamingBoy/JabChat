using System.CommandLine;
using Cli.Commands.Repl.Roaster;
using Spectre.Console;
using XMPP.Core;

namespace Cli.Commands.Repl;

public class RoasterCommand : Command
{
  public RoasterCommand(IXmppClient client) : base("roaster", "Manage the user XMPP roaster")
  {
    Subcommands.Add(new GetCommand(client));
  }
}