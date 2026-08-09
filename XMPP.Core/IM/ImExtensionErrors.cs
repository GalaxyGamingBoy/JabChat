using XMPP.Core.Errors;

namespace XMPP.Core.IM;

public static class UpsertRosterItemResults
{
  public record DuplicateGroups : IClientError
  {
    public string What() => "The roster item contains duplicate group entries.";
  }

  public record LengthLimit : IClientError
  {
    public string What() => "The length of the 'name' attribute is greater than a server-configured limit, or the XML character data of the <group/> element is larger than a server-configured limit.";
  }
}

public static class DeleteRosterItemResults
{
  public record ItemNotFound : IClientError
  {
    public string What() => "The roster item could not be found.";
  }
}