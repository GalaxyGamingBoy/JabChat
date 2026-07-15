using XMPP.Core.ClientErrors;

namespace XMPP.Core.Errors;

// public enum BackendConnectResults
// {
  // ClientAlreadyConnected,
  // AddressPortInvalid,
  // ConnectionFailure
// }

public static class BackendConnectResults
{
  public record ClientAlreadyConnected : IClientError
  {
    public string What() => "XMPP Client already connected";
  }

  public record AddressPortInvalid : IClientError
  {
    public string What() => "XMPP Address Port is invalid";
  }

  public record ConnectionFailure : IClientError
  {
    public string What() => "Failed to connect to XMPP server";
  }
}