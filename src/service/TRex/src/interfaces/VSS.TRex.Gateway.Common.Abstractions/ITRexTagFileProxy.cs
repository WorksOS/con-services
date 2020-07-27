using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  public interface ITRexTagFileProxy
  {
    Task<ContractExecutionResult> SendTagFile(CompactionTagFileRequest compactionTagFileRequest, IHeaderDictionary customHeaders = null);
  }
}
