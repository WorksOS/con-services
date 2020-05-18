using System;
using System.Collections.Generic;
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
      ILogger log, IHeaderDictionary customHeaders,
      bool isDirectSubmission = true)
    {
      var tRexResult = new ContractExecutionResult();

      try
      {
        if (isDirectSubmission)
          tRexResult = await tagFileProxy.SendTagFileDirect(compactionTagFileRequest, customHeaders);
        else
          tRexResult = await tagFileProxy.SendTagFileNonDirect(compactionTagFileRequest, customHeaders);

        return tRexResult;
      }
      catch (Exception e)
      {
        log.LogError(e, $"SendTagFileToTRex: returned exception");
      }

      return new ContractExecutionResult((int)TAGProcServerProcessResultCode.Unknown);
    }
  }
}
