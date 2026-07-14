using System.Security.Cryptography;
using System.Xml.Linq;
using XMPP.Core.Address;
using XMPP.Core.ClientErrors;
using XMPP.Core.SaslErrors;

namespace XMPP.Core.SaslMechanisms;

public class ScramSha384PlusSaslMechanism : ScramPlusSaslMechanism
{
  public override int Priority => 250;
  protected override string MechanismName => "SHA-384";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA384;
  protected override int HashByteLength => 48;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA384(k);
  protected override Func<byte[], byte[]> HashFactory => SHA384.HashData;
}