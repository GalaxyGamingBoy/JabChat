using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public sealed class ScramSha512SaslMechanism : ScramSaslMechanism
{
  public override int Priority => 200;
  protected override string MechanismName => "SHA-512";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA512;
  protected override int HashByteLength => 64;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA512(k);
  protected override Func<byte[], byte[]> HashFactory => SHA512.HashData;
}