using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITRexCompactionDataProxy
  {
    Task<ContractExecutionResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest,
      IDictionary<string, string> customHeaders = null);

    Task<ContractExecutionResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
