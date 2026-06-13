using System.CommandLine;
using Spectre.Console;
using XMPP.Core.Address;

namespace Cli.Commands.Address;

public class GetCommand : Command
{
  private readonly Argument<string> _host = new("host")
  {
    Description = "The hostname of the XMPP host",
    Arity = ArgumentArity.ExactlyOne
  };
  
  private readonly Option<int> _timeout = new("--timeout", "-t")
  {
    Description = "Timeout (in seconds)",
    DefaultValueFactory = _ => 40
  };

  private async Task CommandAction(ParseResult result)
  {
      var host = result.GetRequiredValue(_host);
      var timeout = result.GetValue(_timeout);
      
      var provider = new XmppAddressProvider
      {
        Timeout = timeout
      };

      AnsiConsole.MarkupLine($"Searching address of host: [yellow]{host}[/]");
      
      XmppAddress? addr = null;
      await AnsiConsole.Status()
        .StartAsync("Querying DNS for details",
          async (_) => addr = await provider.GetAddressAsync(host));

      AnsiConsole.MarkupLine(addr is null
        ? "[bold red]No XMPP address found.[/]"
        : $"Host [yellow]{host}[/] found at: [bold green]{addr.Ip}:{addr.Port}[/]");
  }

  public GetCommand() : base("get", "Gets the preferred XMPP address of a host")
  {
    base.Add(_host);
    base.Add(_timeout);
    base.SetAction(CommandAction);
  }
}