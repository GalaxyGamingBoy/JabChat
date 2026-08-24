namespace XMPP.Core;

public abstract class XmppClientExtension : IXmppClientExtension
{
  public virtual Task OnEnable()
  {
    return Task.CompletedTask;
  }

  public virtual Task OnSocketConnected()
  {
    return Task.CompletedTask;
  }

  public virtual Task BeforeSasl()
  {
    return Task.CompletedTask;
  }

  public virtual Task AfterSasl()
  {
    return Task.CompletedTask;
  }

  public virtual Task BeforeBind()
  {
    return Task.CompletedTask;
  }

  public virtual Task AfterBind()
  {
    return Task.CompletedTask;
  }

  public virtual Task OnConnected()
  {
    return Task.CompletedTask;
  }

  public virtual Task OnDisconnected()
  {
    return Task.CompletedTask;
  }

  public virtual ValueTask DisposeAsync()
  {
    return new ValueTask();
  }
}