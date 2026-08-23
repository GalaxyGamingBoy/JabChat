using System.CommandLine;
using Cli.Commands;
using Microsoft.Extensions.Logging;
using Serilog;
using XMPP.Core;

var template =
  "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

var logger = new LoggerConfiguration()
  .MinimumLevel.Debug()
  .Destructure.ByTransforming<EventId>(e => e.Id)
  .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, outputTemplate: template)
  .CreateLogger();

var logFactory = LoggerFactory.Create(b => b.AddSerilog(logger, dispose: true));
JabChatLogging.Factory = logFactory;

var root = new RootCommand("Command Line Utility to interact with XMPP");

var addressCommand = new AddressCommand();
root.Subcommands.Add(addressCommand);

var clientCommand = new ClientCommand();
root.Subcommands.Add(clientCommand);

var result = root.Parse(args);
foreach (var parseError in result.Errors)
  Console.Error.WriteLine(parseError.Message);
  
return await result.InvokeAsync();