namespace XMPP.Core;

public interface IXmppStanzaKey;
  
public interface IXmppStanzaKey<T> : IXmppStanzaKey where T : IXmppStanzaKey<T>
{
  public static abstract string ToStanzaKey();
}