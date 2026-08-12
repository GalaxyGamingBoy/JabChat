namespace XMPP.Core.IM;

public record PresenceUpdate(PresenceShow Show = PresenceShow.None, string? Status = null, int Priority = 0);