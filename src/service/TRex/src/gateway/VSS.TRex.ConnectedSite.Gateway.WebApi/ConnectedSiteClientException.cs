using System;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class ConnectedSiteClientException : Exception
  {
    public ConnectedSiteClientException() { }

    public ConnectedSiteClientException(string message) : base(message) { }

    public ConnectedSiteClientException(string message, Exception inner)
      : base(message, inner) { }
  }
}
