using System.CommandLine;
using System.Reflection.Metadata;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Backend;
using XMPP.Core.Errors;
using XMPP.Core.Presence;

namespace Cli.Commands.Client;

public class ConnectCommand : Command
{
  private readonly Argument<string> _host = new("host")
  {
    Description = "The hostname of the XMPP host",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _username = new("username")
  {
    Description = "The username to use",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _password = new("password")
  {
    Description = "The password to use",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _resource = new("--resource")
  {
    Description = "The resource to use",
    DefaultValueFactory = (_) => Guid.NewGuid().ToString(),
    Arity = ArgumentArity.ZeroOrOne
  };

  private readonly Option<XmppBackend> _backend = new("--backend")
  {
    Description = "The XMPP backend to use",
    DefaultValueFactory = (_) => XmppBackend.Tcp,
    Arity = ArgumentArity.ZeroOrOne
  };

  private RootCommand _replCommands = null!;

  private void CreateReplCommands(IXmppClient client)
  {
    var root = new RootCommand("JabChat CLI REPL");
    
    root.Subcommands.Add(new Repl.StateCommand(client));
    root.Subcommands.Add(new Repl.ReadLockCommand(client));
    root.Subcommands.Add(new Repl.RosterCommand(client));
    root.Subcommands.Add(new Repl.PresenceCommand(client));
    
    _replCommands = root;
  }

  private async Task Repl(IXmppClient client)
  {
    while (true)
    {
      Console.Write("terminal> ");
      var input = Console.ReadLine();

      if (input is null) return;
      if (input is "exit" or "quit" or "q") return;

      await _replCommands.Parse(input).InvokeAsync();
    }
  }

  private async Task CommandAction(ParseResult result)
  {
    var host = result.GetRequiredValue(_host);
    var backend = result.GetRequiredValue(_backend);
    var username = result.GetRequiredValue(_username);
    var password = result.GetRequiredValue(_password);
    var resource = result.GetRequiredValue(_resource);

    var clientResult = await new XmppClientBuilder()
      .UseHost(host)
      .UseBackend(backend)
      .UseUsername(username)
      .UsePassword(password)
      .UseResourceForBinding(resource)
      .BuildAsync();

    if (!clientResult.IsT0)
    {
      AnsiConsole.MarkupLine($"Error(s) occured while connecting to the XMPP host [yellow]{host}[/]");
      IClientError error = (IClientError) clientResult.Value;
      AnsiConsole.MarkupLine($"[bold red]{error.What()}[/]");
      
      return;
    }

    var client = (IXmppClient)clientResult.Value;
    client.ClientErrorRaised += (_, err) =>
    {
      AnsiConsole.MarkupLine($"[bold red]ERR: {err.Error.What()}[/]");
    };
    client.OnPresenceReceived += (_, prs) =>
    {
      if (prs.Presence.Type == PresenceType.Error)
        foreach (var error in prs.Presence.StanzaError!.Errors)
          AnsiConsole.MarkupLine($"[bold red]* {error.What()}[/]");
      else
        AnsiConsole.MarkupLine($"[bold cyan](presence)[/] {prs.Presence}");
    };
    client.OnMessageReceived += (_, msg) =>
    {
      AnsiConsole.MarkupLine($"[cyan](message)[/] {msg.Message}");
    };
    
    AnsiConsole.MarkupLine($"Connecting to [yellow]{host}[/]... (Press any key to exit)");
    await client.ConnectAsync();
    
    while (client.State != XmppState.Connected)
      await Task.Delay(100);
    
    CreateReplCommands(client);
    await Repl(client);
  }

  public ConnectCommand() : base("connect", "Connects to the XMPP host")
  {
    Add(_host);
    Add(_username);
    Add(_password);
    Add(_resource);
    Add(_backend);
    SetAction(CommandAction);
  }
}