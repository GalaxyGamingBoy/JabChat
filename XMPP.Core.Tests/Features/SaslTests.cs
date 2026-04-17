using System.Xml.Serialization;
using XMPP.Core.Features;

namespace XMPP.Core.Tests.Features;

public class SaslTests
{
  [Fact]
  public void SaslFeature_Deserializes()
  {
    const string xml = """
                       <mechanisms xmlns='urn:ietf:params:xml:ns:xmpp-sasl'>
                         <mechanism>EXTERNAL</mechanism>
                         <mechanism>SCRAM-SHA-1-PLUS</mechanism>
                         <mechanism>SCRAM-SHA-1</mechanism>
                         <mechanism>PLAIN</mechanism>
                       </mechanisms>
                       """;
    
    var serializer = new XmlSerializer(typeof(SaslFeature));
    var o = serializer.Deserialize(new StringReader(xml)) as SaslFeature;
    
    Assert.NotNull(o);
    Assert.Equal(o.Mechanisms, ["EXTERNAL", "SCRAM-SHA-1-PLUS", "SCRAM-SHA-1", "PLAIN"]);
  }
}