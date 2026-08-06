using System.Collections.Immutable;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Tls;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using CertificateRequest = Org.BouncyCastle.Tls.CertificateRequest;

namespace XMPP.Core.StartTls;

public class XmppTlsAuthentication(string host) : TlsAuthentication
{
  public void NotifyServerCertificate(TlsServerCertificate serverCertificate)
  {
    var parser = new X509CertificateParser();
    var certs = serverCertificate.Certificate.GetCertificateList()
      .Select(c => parser.ReadCertificate(c.GetEncoded())).ToList();

    var leaf = certs.First();
    if (leaf is null)
      throw new TlsException("Leaf certificate not found");
    
    // 1. Check Certificate Validity
    leaf.CheckValidity(DateTime.UtcNow);

    // 2. Check Certificate Chain of Trust
    using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
    store.Open(OpenFlags.ReadOnly);
    using var userStore = new X509Store(StoreLocation.CurrentUser);
    userStore.Open(OpenFlags.ReadOnly);
    
    var rootTrustAnchors = store.Certificates.Concat(userStore.Certificates)
      .Select(c => parser.ReadCertificate(c.RawData))
      .Select(c => new TrustAnchor(c, null))
      .ToHashSet();

    var serverStore = CollectionUtilities.CreateStore(certs);
    
    var leafSelector = new X509CertStoreSelector {Certificate =  leaf};
    var pkixBuilderParameters = new PkixBuilderParameters(rootTrustAnchors, leafSelector)
    {
      IsRevocationEnabled = false
    };
    pkixBuilderParameters.AddStoreCert(serverStore);

    new PkixCertPathBuilder().Build(pkixBuilderParameters);
    
    // 3. Check Certificate basicConstraints
    if (leaf.GetBasicConstraints() != -1)
      throw new TlsException("Leaf certificate must not be a CA");

    // 4. Check for SAN
    var san = leaf.GetSubjectAlternativeNames().Where(c => c.Contains(host)).ToImmutableList();
    if (san.Count <= 0)
      throw new TlsException("Leaf certificate SAN not found");
  }

  public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
  {
    return null;
  }
}