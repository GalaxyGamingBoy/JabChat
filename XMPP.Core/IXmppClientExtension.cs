namespace XMPP.Core;

public interface IXmppClientExtension
{
  #region Extension Lifecycle
  
  Task OnEnable();
  Task OnSocketConnected();
  Task BeforeSasl();
  Task AfterSasl();
  Task BeforeBind();
  Task AfterBind();
  Task OnConnected();
  Task OnDisconnected();
  
  #endregion
}

public interface IXmppClientExtension<out T> : IAsyncDisposable, IXmppClientExtension where T: class
{
  public static abstract int ExtensionIdentifier { get; }

  static abstract T Create(IXmppClient client);
}