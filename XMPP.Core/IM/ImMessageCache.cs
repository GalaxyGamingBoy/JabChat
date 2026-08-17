using XMPP.Core.Messages;

namespace XMPP.Core.IM;

public record ImMessageCache(string FromFullJid, MessageThread Thread);