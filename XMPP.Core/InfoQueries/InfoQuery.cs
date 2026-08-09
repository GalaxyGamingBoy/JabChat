using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using XMPP.Core.StanzaErrors;

namespace XMPP.Core.InfoQueries;

[XmlRoot("iq", Namespace = "jabber:client")]
public record InfoQuery
{
  public InfoQuery()
  {
  }

  [SetsRequiredMembers]
  public InfoQuery(InfoQueryType type)
  {
    Type = type;
  }

  [XmlAttribute("id")]
  public string? Id { get; set; }
  
  [XmlAttribute("type")]
  public required InfoQueryType Type { get; init; }
  
  [XmlAttribute("to")]
  public string? To;
  
  [XmlAttribute("from")]
  public string? From;
  
  [XmlElement("error")]
  public StanzaError? StanzaError { get; init; }
  
  [XmlElement("bind", Namespace = "urn:ietf:params:xml:ns:xmpp-bind")]
  public Bind? ResourceBind { get; init; }

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