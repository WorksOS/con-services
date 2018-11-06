using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.ResultHandling
{
  public class ConnectedSiteMessageResult : ContractExecutionResult
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private ConnectedSiteMessageResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static ConnectedSiteMessageResult Create(int code, string message)
    {
      return new ConnectedSiteMessageResult()
      {
        Code = code,
        Message = message
      };

    }
  }
}
