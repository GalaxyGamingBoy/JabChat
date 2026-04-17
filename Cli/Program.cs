using System.CommandLine;
using Spectre.Console;
using XMPP.Core.Address;

var root = new RootCommand("Command Line Utility to interact with XMPP");

{
  var addressCommand = new Command("address", "XMPP address utilities");
  root.Subcommands.Add(addressCommand);
  
  var hostArgument = new Argument<string>("host")
  {
    Description = "The hostname of the XMPP host",
    Arity = ArgumentArity.ExactlyOne
  };
  
  {
    var timeoutOption = new Option<int>("--timeout", "-t")
    {
      Description = "Timeout (in seconds)",
      DefaultValueFactory = _ => 40
    };
    
    var getCommand = new Command("get", "Gets the preferred XMPP address of a host")
    {
      hostArgument,
      timeoutOption
    };

    getCommand.SetAction(async (result) =>
    {
      var host = result.GetRequiredValue(hostArgument);
      var timeout = result.GetValue(timeoutOption);
      
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
    });
    
    addressCommand.Subcommands.Add(getCommand);
  }

  {
    var srvCommand = new Command("srv", "Gets the XMPP SRV Records of a host") { hostArgument };

    srvCommand.SetAction(async (result) =>
    {
      var host = result.GetRequiredValue(hostArgument);

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
    });
    
    addressCommand.Subcommands.Add(srvCommand);
  }
}

var result = root.Parse(args);
foreach (var parseError in result.Errors)
  Console.Error.WriteLine(parseError.Message);
  
return await result.InvokeAsync();