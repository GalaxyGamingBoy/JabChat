namespace XMPP.Core.IM;

public class ImExtension(IXmppClient client) : IXmppClientExtension<ImExtension>
{
  public static int ExtensionIdentifier => 0;

  public static ImExtension Create(IXmppClient client)
  {
    return new ImExtension(client);
  }

  public async Task GetRoaster()
  {
    Console.WriteLine("Getting roaster...");
    _ = client.State;
    await Task.Delay(100);
  }

  public ValueTask DisposeAsync()
  {
    return new ValueTask();
  }

}