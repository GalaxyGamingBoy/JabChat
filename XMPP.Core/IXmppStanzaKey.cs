using System.Xml.Serialization;

namespace XMPP.Core;

public interface IXmppStanzaKey;
  
public interface IXmppStanzaKey<T> : IXmppStanzaKey where T : IXmppStanzaKey<T>
{
  public static abstract string ToStanzaKey();
}

public interface IDefaultStanzaKey<T> : IXmppStanzaKey<T> where T : IXmppStanzaKey<T>
{
  static string IXmppStanzaKey<T>.ToStanzaKey()
  {
    var attr = (XmlRootAttribute?)Attribute.GetCustomAttribute(
      typeof(T), typeof(XmlRootAttribute));
    
    if (attr is null)
      throw new Exception("XML Attribute not found.");
    if (attr.Namespace is null)
      throw new Exception("XML Attribute Namespace is null.");
    if (attr.ElementName is null)
      throw new Exception("XML Element Name is null.");
    
    
    return $"{attr.Namespace}/{attr.ElementName}";
  }
}