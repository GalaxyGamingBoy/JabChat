using Microsoft.Extensions.Logging;

namespace XMPP.Core.LogMessages;

public static partial class ScramPlusSaslMechanismLogs
{
  [LoggerMessage(LogLevel.Information, "Sending GS2 header to server with identity and nonce")]
  public static partial void SendingHeader(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, "Received authentication challenge")]
  public static partial void ReceivedChallenge(ILogger logger);
  
  [LoggerMessage(LogLevel.Error, "Provided nonce from authentication challenge is mismatched!\n")]
  public static partial void MismatchedNonce(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, "Computing challenge proof")]
  public static partial void ComputingProof(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, "Sending challenge completion proof to server\n")]
  public static partial void SendingProof(ILogger logger);
  
  [LoggerMessage(LogLevel.Error, "Provided server signature is mismatched to computed one!")]
  public static partial void MismatchedServerSignature(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, "SASL authentication confirmation received")]
  public static partial void SaslCompleted(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, "Using channel bind: {ChannelBind}")]
  public static partial void UseChannelBind(ILogger logger, string channelBind);
}