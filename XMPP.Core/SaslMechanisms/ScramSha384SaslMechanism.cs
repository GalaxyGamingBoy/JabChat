using System.Security.Cryptography;

namespace XMPP.Core.SaslMechanisms;

public sealed class ScramSha384SaslMechanism : ScramSaslMechanism
{
  public override int Priority => 300;
  protected override string MechanismName => "SHA-384";
  protected override HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA384;
  protected override int HashByteLength => 48;
  protected override Func<byte[], HMAC> HmacFactory => (k) => new HMACSHA384(k);
  protected override Func<byte[], byte[]> HashFactory => SHA384.HashData;
}