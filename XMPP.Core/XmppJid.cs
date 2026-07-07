namespace XMPP.Core;

public record XmppJid
{
  public required string LocalPart;
  public required string DomainPart;
  public required string Resource;

  public override string ToString()
  {
    return $"{LocalPart}@{DomainPart}/{Resource}";
  }
}