using System.CommandLine;
using Cli.Commands.Client;

namespace Cli.Commands;

public class ClientCommand : Command
{
  private readonly ConnectCommand _connectCommand = new ConnectCommand();
  
  public ClientCommand() : base("client", "Interact with the XMPP client")
  {
    Subcommands.Add(_connectCommand);
  }
}