using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace XMPP.Core;

public static class JabChatLogging
{
  public static ILoggerFactory Factory = NullLoggerFactory.Instance;
}