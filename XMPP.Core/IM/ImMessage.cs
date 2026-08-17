using XMPP.Core.Messages;

namespace XMPP.Core.IM;

public record ImMessage(string ToBare, string Body, string? Subject);