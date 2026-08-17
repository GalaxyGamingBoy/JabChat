using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public sealed class ScramSha3512SaslMechanism : ScramSaslMechanism
{
  public override int Priority => 100;
  protected override string MechanismName => "SHA3-512";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA3_512;
  protected override int HashByteLength => 64;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA3_512(k);
  protected override Func<byte[], byte[]> HashFactory => SHA3_512.HashData;
}