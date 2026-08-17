using System.CommandLine;
using Spectre.Console;
using XMPP.Core.Address;

namespace Cli.Commands.Address;

public class SrvCommand : Command
{
  private readonly Argument<string> _host = new("host")
  {
    Description = "The hostname of the XMPP host",
    Arity = ArgumentArity.ExactlyOne
  };

  private async Task CommandAction(ParseResult result)
  {
    var host = result.GetRequiredValue(_host);

    var resolver = new XmppAddressResolver();

    AnsiConsole.MarkupLine($"Fetching XMPP SRV records of host: [yellow]{host}[/]");

    List<XmppAddressSrv> recs = [];
    await AnsiConsole.Status()
      .StartAsync("Querying DNS for details",
        async (_) => recs = await resolver.ResolveAddressFromSrvAsync(host));

    AnsiConsole.MarkupLine($"Host [yellow]{host}[/] XMPP SRV records: ");
    if (recs.Count == 0)
      AnsiConsole.MarkupLine("[bold red]No SRV records found.[/]");
      
    foreach (var rec in recs)
      AnsiConsole.MarkupLine(
        $"- [bold blue]{rec.Host}[/]:[blue]{rec.Port}[/] (Priority: {rec.Priority}, Weight: {rec.Weight})");
  }

  public SrvCommand() : base("srv", "Gets the XMPP SRV Records of a host")
  {
    Add(_host);
    SetAction(CommandAction);
  }
}