namespace XMPP.Core;

public interface IXmppClientExtension;

public interface IXmppClientExtension<out T> : IAsyncDisposable, IXmppClientExtension where T: class
{
  public static abstract int ExtensionIdentifier { get; }

  static abstract T Create(IXmppClient client);
}