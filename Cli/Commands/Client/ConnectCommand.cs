using System.CommandLine;
using System.Globalization;
using Spectre.Console;
using XMPP.Core;
using XMPP.Core.Address;

namespace Cli.Commands.Client;

public class ConnectCommand : Command
{
  private readonly Argument<string> _host = new("host")
  {
    Description = "The hostname of the XMPP host",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _jid = new("jid")
  {
    Description = "The JID to use",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Argument<string> _password = new("password")
  {
    Description = "The password to use",
    Arity = ArgumentArity.ExactlyOne
  };

  private readonly Option<XmppBackend> _backend = new("--backend")
  {
    Description = "The XMPP backend to use",
    DefaultValueFactory = (_) => XmppBackend.Tcp
  };

  private async Task CommandAction(ParseResult result)
  {
    var host = result.GetRequiredValue(_host);
    var backend = result.GetRequiredValue(_backend);
    var jid = result.GetRequiredValue(_jid);
    var password = result.GetRequiredValue(_password);

    var client = await new XmppClientBuilder()
      .UseHost(host)
      .UseBackend(backend)
      .UseJid(jid)
      .UsePassword(password)
      .BuildAsync();

    if (client.IsFailed)
    {
      AnsiConsole.MarkupLine($"Error(s) occured while connecting to the XMPP host [yellow]{host}[/]");
      foreach (var error in client.Errors)
        AnsiConsole.MarkupLine($"[bold red]{error.Message}[/]");
      
      return;
    }

    client.Value.ClientErrorRaisedAsync += (sender, err) =>
    {
      AnsiConsole.MarkupLine($"[bold red]ERR: {err.Error.What()}[/]");
    };
    
    AnsiConsole.MarkupLine($"Connecting to [yellow]{host}[/]... (Press any key to exit)");
    await client.Value.ConnectAsync();

    Console.Read();
  }

  public ConnectCommand() : base("connect", "Connects to the XMPP host")
  {
    base.Add(_host);
    base.Add(_jid);
    base.Add(_password);
    base.Add(_backend);
    
    base.SetAction(CommandAction);
  }
}