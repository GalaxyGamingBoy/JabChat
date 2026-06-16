namespace XMPP.Core;

public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);