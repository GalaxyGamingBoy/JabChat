using System.Xml;
using System.Xml.Serialization;

namespace XMPP.Core;

public record StanzaExtensions
{
  [XmlAnyElement]
  public List<XmlElement> ExtensionElements { get; set; } = [];

  [XmlIgnore]
  public Dictionary<string, IXmppStanzaKey> ExtensionObjects { get; } = [];

  public void AddExtensionObject<T>(T obj) where T : IXmppStanzaKey<T>
  {
    var serializer = XmppClientRegistry.InfoQuerySerializers[T.ToStanzaKey()];
    var document = new XmlDocument();
    
    using (var writer = document.CreateNavigator()!.AppendChild())
    {
      serializer.Serialize(writer, obj);
    }

    ExtensionElements.Add(document.DocumentElement!);
  }

  public T? GetExtensionObject<T>() where T : class, IXmppStanzaKey<T>
  {
    ExtensionObjects.TryGetValue(T.ToStanzaKey(), out var extensionObject);
    return extensionObject as T;
  }
  
  public void DeserializeExtensions()
  {
    foreach (var extensionObject in ExtensionElements)
    {
      var key = $"{extensionObject.NamespaceURI}/{extensionObject.LocalName}";
      var serializer = XmppClientRegistry.InfoQuerySerializers[key];
      using var reader = new XmlNodeReader(extensionObject);
      ExtensionObjects[key] = (serializer.Deserialize(reader) as IXmppStanzaKey)!;
    }
  }
}