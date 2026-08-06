namespace XMPP.Core;

public record XmppJid(string LocalPart, string DomainPart, string? Resource)
{
  public override string ToString()
  {
    return Resource is not null
      ? $"{LocalPart}@{DomainPart}/{Resource}" : $"{LocalPart}@{DomainPart}";
  }
}