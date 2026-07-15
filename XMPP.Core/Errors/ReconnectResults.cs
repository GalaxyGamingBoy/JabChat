using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

public static class ReconnectResults
{
  public record ClientAlreadyConnected : IClientError
  {
    public string What() => "XMPP client is already connected";
  }

  public record AddressPortInvalid : IClientError
  {
    public string What() => "XMPP Address Port is invalid";
  }

  public record ReconnectionFailure : IClientError
  {
    public string What() => "Failed to reconnect to XMPP server";
  }
}