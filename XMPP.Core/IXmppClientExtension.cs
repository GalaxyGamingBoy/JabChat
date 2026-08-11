namespace XMPP.Core;

public interface IXmppClientExtension
{
  Task ActivateAsync();
}

public interface IXmppClientExtension<out T> : IAsyncDisposable, IXmppClientExtension where T: class
{
  public static abstract int ExtensionIdentifier { get; }
  public static abstract XmppClientExtensionActivateOn ActivateOn { get; }

  static abstract T Create(IXmppClient client);
}