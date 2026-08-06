namespace XMPP.Core.Errors;

public static class SendInfoQueryResults
{
  public record SerializationFailure : IClientError
  {
    public string What() => "Failed to serialize the XMPP stanza";
  }

  public record WriterNullException : IClientError
  {
    public string What() => "The XMPP client writer is null";
  }

  public record InfoQueryError(string Error) : IClientError
  {
    public string What() => "An error was returned from the Info Query";
  };
}