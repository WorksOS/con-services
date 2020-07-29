using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Gateway.Common.Abstractions;
using Microsoft.AspNetCore.Http;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  public static class TagFileHelper
  {
    /// <summary>
    /// Sends tag file to TRex endpoint, retrieving result 
    /// </summary>
    public static async Task<ContractExecutionResult> SendTagFileToTRex(CompactionTagFileRequest compactionTagFileRequest,
      ITRexTagFileProxy tagFileProxy,
      ILogger log, IHeaderDictionary customHeaders)
    {
      try
      {
        return await tagFileProxy.SendTagFile(compactionTagFileRequest, customHeaders);
      }
      catch (Exception e)
      {
        log.LogError(e, $"{nameof(SendTagFileToTRex)}: returned exception");
      }

      return new ContractExecutionResult((int)TRexTagFileResultCode.TRexUnknownException);
    }
  }
}
