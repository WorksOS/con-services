using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
