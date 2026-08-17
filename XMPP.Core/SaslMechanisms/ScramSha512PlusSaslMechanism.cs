using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public class ScramSha512PlusSaslMechanism : ScramPlusSaslMechanism
{
  public override int Priority => 150;
  protected override string MechanismName => "SHA-512";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA512;
  protected override int HashByteLength => 64;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA512(k);
  protected override Func<byte[], byte[]> HashFactory => SHA512.HashData;
}