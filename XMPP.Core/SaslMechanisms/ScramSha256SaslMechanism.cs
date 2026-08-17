using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public class ScramSha256SaslMechanism : ScramSaslMechanism
{
  public override int Priority => 400;
  protected override string MechanismName => "SHA-256";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA256;
  protected override int HashByteLength => 32;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA256(k);
  protected override Func<byte[], byte[]> HashFactory => SHA256.HashData;
}