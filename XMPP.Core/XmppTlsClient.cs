using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace XMPP.Core;

public class XmppTlsClient(string host) : DefaultTlsClient(new BcTlsCrypto(new SecureRandom()))
{
  public override TlsAuthentication GetAuthentication()
  {
    return new XmppTlsAuthentication(host);
  }
}