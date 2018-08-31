using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TAGProcServiceDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{

  /// <summary>
  ///
  /// </summary>
  public class TagFileHelper
  {
    /// <summary>
    /// Sends tag file to TRex endpoint, retrieving result 
    /// </summary>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> SendTagFileToTRex(CompactionTagFileRequest compactionTagFileRequest,
      ITRexTagFileProxy tagFileProxy,
      ILogger log, IDictionary<string, string> customHeaders,
      bool isDirectSubmission = true)
    {
      var tRexResult = new ContractExecutionResult();

      try
      {
        if (isDirectSubmission)
        {
          tRexResult = await tagFileProxy.SendTagFileDirect(compactionTagFileRequest, customHeaders).ConfigureAwait(false);
        }
        else
        {
          tRexResult = await tagFileProxy.SendTagFileNonDirect(compactionTagFileRequest, customHeaders).ConfigureAwait(false);
        }

        return tRexResult;
      }
      catch (Exception e)
      {
        log.LogError($"SendTagFileToTRex: returned exception: {e.Message}");
      }

      return new ContractExecutionResult((int) TTAGProcServerProcessResult.tpsprUnknown);
    }
  }
}

