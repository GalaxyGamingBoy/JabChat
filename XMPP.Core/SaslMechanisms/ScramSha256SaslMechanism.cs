using System.Security.Cryptography;
using System.Xml.Linq;
using XMPP.Core.Address;

namespace XMPP.Core.SaslMechanisms;

public class ScramSha256SaslMechanism : ISaslMechanism
{
  public string Mechanism => "SCRAM-SHA-256";
  public int Priority => 100;

  private IXmppClient _client = null!;
  private XmppCreds _credentials = null!;
  private string _nonce = string.Empty;

  private string _clientFirstBare = string.Empty;
  private string _serverFirst = string.Empty;
  private string _clientFinalNoProof = string.Empty;
  
  private const int Sha256ByteLength = 32;
  
  private string _serverSignature = string.Empty;

  public void BindClient(IXmppClient client)
  {
    _client = client;
  }

  private byte[] ComputeSaltedPassword(byte[] salt, int iterations)
  {
    return Rfc2898DeriveBytes.Pbkdf2(
      _credentials.Password,
      salt,
      iterations,
      HashAlgorithmName.SHA256, 
      Sha256ByteLength
      );
  }

  private byte[] ComputeClientKey(byte[] saltedPassword)
  {
    using var hmac = new HMACSHA256(saltedPassword);
    return hmac.ComputeHash("Client Key"u8.ToArray());
  }
  
  private byte[] ComputeServerKey(byte[] saltedPassword)
  {
    using var hmac = new HMACSHA256(saltedPassword);
    return hmac.ComputeHash("Server Key"u8.ToArray());
  }

  private byte[] ComputeSignature(byte[] storedKey, string authMessage)
  {
    using var hmac = new HMACSHA256(storedKey);
    var authBytes = System.Text.Encoding.UTF8.GetBytes(authMessage);
    return hmac.ComputeHash(authBytes);
  }
  
  private byte[] Hash(byte[] val) => SHA1.HashData(val);

  private async void OnChallengerReceived(object sender, object? challengeMessageReceived)
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
      // todo: throw err
      Console.WriteLine($"Aborting {Mechanism}, challenge nonce mismatch.");
      return;
    }

    _clientFinalNoProof = $"c=biws,r={challengeNonce}";
    
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

    var message = $"c=biws,r={challengeNonce},p={proof}";
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "response");

    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(System.Convert.ToBase64String(bytes));

    await _client.SendStanzaAsync(element);
    _client.ReadLock.Release();
  }

  private async void OnSuccessReceived(object sender, object? successMessageReceived)
  {
    var successMessage = (SaslSuccess)successMessageReceived!;
    var messageBytes = Convert.FromBase64String(successMessage.Body);
    var message = System.Text.Encoding.UTF8.GetString(messageBytes);

    if (message != $"v={_serverSignature}")
    {
      // todo: throw err
      Console.WriteLine($"Aborting {Mechanism}, server signature mismatch.");
      return;
    }

    await _client.StopBackgroundService();
    await _client.SaslCompleted();
    _client.StartBackgroundService();
    _client.ReadLock.Release();
  }


  public async Task Use(XmppCreds credentials)
  {
    _nonce = Guid.NewGuid().ToString();
    _credentials = credentials;
    
    _client.RegisterUnexpectedStanza<ScramChallenge>(OnChallengerReceived);
    _client.RegisterUnexpectedStanza<SaslSuccess>(OnSuccessReceived);
    
    var localpart = credentials.Jid.Split("@")[0];
    _clientFirstBare = $"n={localpart},r={_nonce}";
    var message = $"n,,{_clientFirstBare}";
    
    XNamespace ns = "urn:ietf:params:xml:ns:xmpp-sasl";
    var element = new XElement(ns + "auth");
    element.SetAttributeValue("mechanism", Mechanism);
    
    var bytes = System.Text.Encoding.UTF8.GetBytes(message);
    element.SetValue(Convert.ToBase64String(bytes));
    
    await _client.SendStanzaAsync(element);
  }
}