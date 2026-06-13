using System.CommandLine;
using Cli.Commands;

var root = new RootCommand("Command Line Utility to interact with XMPP");

var addressCommand = new AddressCommand();
root.Subcommands.Add(addressCommand);

var result = root.Parse(args);
foreach (var parseError in result.Errors)
  Console.Error.WriteLine(parseError.Message);
  
return await result.InvokeAsync();