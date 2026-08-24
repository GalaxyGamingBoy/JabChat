using Microsoft.Extensions.Logging;

namespace XMPP.Core.LogMessages;

public static partial class PlainSaslMechanismLogs
{
  [LoggerMessage(LogLevel.Information, "Sending plain authentication packet to server")]
  public static partial void SendingPlainAuth(ILogger logger);

  [LoggerMessage(LogLevel.Information, "SASL authentication confirmation received")]
  public static partial void SaslCompleted(ILogger logger);
}