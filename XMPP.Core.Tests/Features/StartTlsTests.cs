using System.Xml.Serialization;
using XMPP.Core.Features;

namespace XMPP.Core.Tests.Features;

public class StartTlsTests
{
  [Fact]
  public void StartTlsFeature_Deserializes_WhenRequired()
  {
    const string xml = """
                       <starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'>
                         <required/>
                       </starttls>
                       """;
    
    var serializer = new XmlSerializer(typeof(StartTlsFeature));
    var o = serializer.Deserialize(new StringReader(xml)) as StartTlsFeature;
    
    Assert.NotNull(o);
    Assert.True(o.IsRequired);
  }

  [Fact]
  public void StartTlsFeature_Deserializes_WhenNotRequired()
  {
    const string xml = "<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls' />";
    
    var serializer = new XmlSerializer(typeof(StartTlsFeature));
    var o = serializer.Deserialize(new StringReader(xml)) as StartTlsFeature;
    
    Assert.NotNull(o);
    Assert.False(o.IsRequired);
  }
}