using System.CommandLine;
using Cli.Commands.Repl.Presence.Request;
using XMPP.Core;

namespace Cli.Commands.Repl.Presence;

public class RequestCommand : Command
{
  public RequestCommand(IXmppClient client) : base("request", "Manage presence requests")
  {
    Subcommands.Add(new ApproveCommand(client));
    Subcommands.Add(new CancelCommand(client));
    Subcommands.Add(new PreApproveCommand(client));
    Subcommands.Add(new SubscribeCommand(client));
    Subcommands.Add(new UnsubscribeCommand(client));
  }
}