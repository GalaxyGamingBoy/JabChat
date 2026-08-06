using System.Security.Cryptography;
using System.Xml.Linq;
using Org.BouncyCastle.Tls;
using XMPP.Core.Backend;
using XMPP.Core.SaslErrors;

namespace XMPP.Core.SaslMechanisms;

public abstract class ScramPlusSaslMechanism : ISaslMechanism
{
  public abstract int Priority { get; }
  protected abstract string MechanismName { get; }
  protected abstract HashAlgorithmName HashAlgorithm { get; }
  protected abstract int HashByteLength { get; }
  protected abstract Func<byte[], HMAC> HmacFactory { get; }
  protected abstract Func<byte[], byte[]> HashFactory { get; }
  
  public string Mechanism => $"SCRAM-{MechanismName}-PLUS"; 
  
  private IXmppClient _client = null!;
  private IXmppClientBackend _backend = null!;
  private XmppCredentials _credentials = null!;
  private string _nonce = string.Empty;

  private string _clientFirstBare = string.Empty;
  private string _serverFirst = string.Empty;
  private string _clientFinalNoProof = string.Empty;
  
  private string _serverSignature = string.Empty;

  public void BindClient(IXmppClient client, IXmppClientBackend backend)
  {
    _client = client;
    _backend = backend;
  }

  private byte[] ComputeSaltedPassword(byte[] salt, int iterations)
  {
    return Rfc2898DeriveBytes.Pbkdf2(
      _credentials.Password,
      salt,
      iterations,
      HashAlgorithm,
      HashByteLength
      );
  }

  private byte[] ComputeClientKey(byte[] saltedPassword)
  {
    using var hmac = HmacFactory(saltedPassword);
    return hmac.ComputeHash("Client Key"u8.ToArray());
  }
  
  private byte[] ComputeServerKey(byte[] saltedPassword)
  {
    using var hmac = HmacFactory(saltedPassword);
    return hmac.ComputeHash("Server Key"u8.ToArray());
  }

  private byte[] ComputeSignature(byte[] storedKey, string authMessage)
  {
    using var hmac = HmacFactory(storedKey);
    var authBytes = System.Text.Encoding.UTF8.GetBytes(authMessage);
    return hmac.ComputeHash(authBytes);
  }
  
  private byte[] Hash(byte[] val) => HashFactory(val);

  private async Task OnChallengerReceived(object sender, object? challengeMessageReceived)
  {
    var challengeMessage = (ScramChallenge)challengeMessageReceived!;
    var deserializedBytes = Convert.FromBase64String(challengeMessage.Body);
    var deserialized = System.Text.Encoding.UTF8.GetString(deserializedBytes);
    _serverFirst = deserialized;

    var parts = deserialized.Split(",");
    var challenge = parts
      .Select(part => part.Split("="))
      .ToDictionary(x => x[0], x => x[1]);
    
    var challengeNonce = challenge["r"];
    int.TryParse(challenge["i"], out var challengeIterations);
    var challengeSalt = Convert.FromBase64String(challenge["s"]);

    if (!challengeNonce.StartsWith(_nonce))
    {
      _client.InvokeClientError(new ChallengeNonceMismatch(Mechanism));
      return;
    }

    var gs2Header = System.Text.Encoding.UTF8.GetBytes($"p={GetChannelBindingTypeText()},,");
    var binding = _backend.GetChannelBindingData();
    var channel = Convert.ToBase64String(gs2Header.Concat(binding).ToArray());
    _clientFinalNoProof = $"c={channel},r={challengeNonce}";
    
    var salted = ComputeSaltedPassword(challengeSalt, challengeIterations);
    var clientKey = ComputeClientKey(salted);
    var serverKey = ComputeServerKey(salted);
    var storedKey = Hash(clientKey);
    
    var authMessage = $"{_clientFirstBare},{_serverFirst},{_clientFinalNoProof}";
    var clientSignature = ComputeSignature(storedKey, authMessage);
    
    var serverSignature = ComputeSignature(serverKey, authMessage);
    _serverSignature = Convert.ToBase64String(serverSignature);
    
    var proofBytes = clientKey.Zip(clientSignature, (f, s) => (byte)(f ^ s)).ToArray();
    var proof = Convert.ToBase64String(proofBytes);

    var message = $"{_clientFinalNoProof},p={proof}";
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "response");

    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(Convert.ToBase64String(bytes));

    await _client.SendStanzaAsync(element);
    _client.ReadLock.Release();

    _client.UnregisterUnexpectedStanza<ScramChallenge>();
  }

  private async Task OnSuccessReceived(object sender, object? successMessageReceived)
  {
    var successMessage = (SaslSuccess)successMessageReceived!;
    var messageBytes = Convert.FromBase64String(successMessage.Body);
    var message = System.Text.Encoding.UTF8.GetString(messageBytes);

    if (message != $"v={_serverSignature}")
    {
      _client.InvokeClientError(new ServerSignatureMismatch(Mechanism));
      return;
    }

    await _client.StopBackgroundService();
    await _client.SaslCompleted();
    _client.StartBackgroundService();
    _client.ReadLock.Release();
    
    _client.UnregisterUnexpectedStanza<SaslSuccess>();
  }

  public async Task Use(XmppCredentials credentials)
  {
    _nonce = Guid.NewGuid().ToString();
    _credentials = credentials;
    
    _client.RegisterUnexpectedStanza<ScramChallenge>(OnChallengerReceived);
    _client.RegisterUnexpectedStanza<SaslSuccess>(OnSuccessReceived);
    
    _clientFirstBare = $"n={credentials.Jid.LocalPart},r={_nonce}";
    var message = $"p={GetChannelBindingTypeText()},,{_clientFirstBare}";
    
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "auth");
    element.SetAttributeValue("mechanism", Mechanism);
    
    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(Convert.ToBase64String(bytes));
    
    Console.WriteLine(GetChannelBindingTypeText());
    
    await _client.SendStanzaAsync(element);
  } 

  private string GetChannelBindingTypeText()
  {
    var version = _backend.ClientProtocolVersion;
    if (version is null)
      return string.Empty;

    return version.Equals(ProtocolVersion.TLSv13)
      ? "tls-exporter"
      : "tls-unique";
  }
}