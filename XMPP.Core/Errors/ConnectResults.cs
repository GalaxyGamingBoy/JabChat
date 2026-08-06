namespace XMPP.Core.Errors;

public static class ConnectResults
{
  public record ClientAlreadyConnected : IClientError
  {
    public string What() => "XMPP client is already connected";
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