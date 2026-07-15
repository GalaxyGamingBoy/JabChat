using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class DisconnectResults
{
  public record StreamNullException : IClientError
  {
    public string What() => "The XMPP client stream is null";
  }

  public record AlreadyDisconnected : IClientError
  {
    public string What() => "The XMPP client stream is already disconnected";
  }
}