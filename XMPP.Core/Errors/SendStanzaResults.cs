using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class SendStanzaResults
{
  public record SerializationFailure : IClientError
  {
    public string What() => "Failed to serialize the XMPP stanza";
  }

  public record WriterNullException : IClientError
  {
    public string What() => "The XMPP client writer is null";
  }
}