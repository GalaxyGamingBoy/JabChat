using Microsoft.Extensions.Logging;

namespace XMPP.Core.LogMessages;

public static partial class TcpXmppBackendLogs
{
  [LoggerMessage(LogLevel.Information, Message = "TCP Backend bound to XMPP client")]
  public static partial void BindToClient(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message =  "TCP Backend disposed or TCP stream disconnected")]
  public static partial void DisposingBackend(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "Attempting TCP connection to {Ip}:{Port}")]
  public static partial void Connect(ILogger logger, string ip, int port);
  
  [LoggerMessage(LogLevel.Information, Message = "Requesting upgrade with StartTLS")]
  public static partial void StartTlsUpgradeRequest(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "Upgrading TCP stream to TLS")]
  public static partial void UpgradingStreamToTls(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "TCP stream successfully upgraded to TLS")]
  public static partial void StreamUpgradedToTls(ILogger logger);
  
  [LoggerMessage(LogLevel.Information, Message = "Server confirmed TLS upgrade")]
  public static partial void ServerConfirmedTlsUpgrade(ILogger logger);
  
  [LoggerMessage(LogLevel.Critical, Message = "Server rejected the TLS upgrade")]
  public static partial void ServerRejectedTlsUpgrade(ILogger logger);
  
  [LoggerMessage(LogLevel.Error, Message = "TLS upgrade failed, no existing TCP connection exists")]
  public static partial void TlsUpgradeFailedNoTcp(ILogger logger);
}