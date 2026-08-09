namespace XMPP.Core;

public interface IXmppClientExtension
{
  /// <summary>
  /// Called when extension is first loaded
  /// </summary>
  Task LoadAsync();
  
  /// <summary>
  /// Called after all of the extensions at this level are loaded 
  /// </summary>
  /// <returns></returns>
  Task ActivateAsync();
}

public interface IXmppClientExtension<out T> : IAsyncDisposable, IXmppClientExtension where T: class
{
  public static abstract int ExtensionIdentifier { get; }
  public static abstract XmppClientExtensionLoadAt LoadAt { get; }

  static abstract T Create(IXmppClient client);
}