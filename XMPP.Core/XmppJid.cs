namespace XMPP.Core;

public record XmppJid
{
  public required string LocalPart;
  public required string DomainPart;
  public string? Resource;

  public override string ToString()
  {
    return Resource is not null
      ? $"{LocalPart}@{DomainPart}/{Resource}" : $"{LocalPart}@{DomainPart}";
  }
}