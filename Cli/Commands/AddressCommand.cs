using System.CommandLine;
using Cli.Commands.Address;

namespace Cli.Commands;

public class AddressCommand : Command
{
  private readonly Command _getCommand = new GetCommand();
  private readonly Command _srvCommand = new SrvCommand();
  
  public AddressCommand() : base("address", "XMPP address utilities")
  {
    Subcommands.Add(_getCommand);
    Subcommands.Add(_srvCommand);
  }
}