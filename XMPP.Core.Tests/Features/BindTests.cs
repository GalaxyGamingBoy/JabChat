using System.Xml.Serialization;
using XMPP.Core.Features;

namespace XMPP.Core.Tests.Features;

public class BindTests  
{
  [Fact]
  public void BindFeature_Deserializes()
  {
    const string xml = "<bind xmlns='urn:ietf:params:xml:ns:xmpp-bind'/>";
    
    var serializer = new XmlSerializer(typeof(BindFeature));
    var o = serializer.Deserialize(new StringReader(xml)) as BindFeature;
    
    Assert.NotNull(o);
  }
}