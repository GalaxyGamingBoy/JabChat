using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public sealed class ScramSha1PlusSaslMechanism : ScramPlusSaslMechanism
{
  public override int Priority => 450;
  protected override string MechanismName => "SHA-1";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA1;
  protected override int HashByteLength => 20;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA1(k);
  protected override Func<byte[], byte[]> HashFactory => SHA1.HashData;
}