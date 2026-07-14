using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace XMPP.Core;

public class XmppTlsClient(string host) : DefaultTlsClient(new BcTlsCrypto(new SecureRandom()))
{
  private byte[] _channelBindingData;
  
  public override TlsAuthentication GetAuthentication()
  {
    return new XmppTlsAuthentication(host);
  }

  public override void NotifyHandshakeComplete()
  {
    if (m_context.ServerVersion == ProtocolVersion.TLSv13)
      _channelBindingData = m_context.ExportChannelBinding(ChannelBinding.tls_exporter);
    else
      _channelBindingData = m_context.ExportChannelBinding(ChannelBinding.tls_unique);
  }

  public ProtocolVersion GetNegotiatedVersion()
  {
    return m_context.ServerVersion;
  }

  public byte[] GetChannelBindingData()
  {
    return _channelBindingData;
  }
}