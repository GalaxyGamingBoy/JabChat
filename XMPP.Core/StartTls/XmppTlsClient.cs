using Org.BouncyCastle.Security;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Tls.Crypto.Impl.BC;

namespace XMPP.Core.StartTls;

public class XmppTlsClient(string host) : DefaultTlsClient(new BcTlsCrypto(new SecureRandom()))
{
  private byte[] _channelBindingData;
  
  public override TlsAuthentication GetAuthentication()
  {
    return new XmppTlsAuthentication(host);
  }

  public override void NotifyHandshakeComplete()
  {
    _channelBindingData =
      m_context.ExportChannelBinding(m_context.ServerVersion.Equals(ProtocolVersion.TLSv13)
      ? ChannelBinding.tls_exporter
      : ChannelBinding.tls_unique);
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