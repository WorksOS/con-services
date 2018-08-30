using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TAGProcServiceDecls;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{

  /// <summary>
  ///
  /// </summary>
  public class TagFileHelper
  {
    /// <summary>
    /// Sends tag file to TRex endpoint, retrieving result (todo how does content of this compared with Raptor?)
    /// </summary>
    /// <returns></returns>
    public static async Task<TagFileDirectSubmissionResult> SendTagFileToTRex(CompactionTagFileRequest compactionTagFileRequest,
      ITRexTagFileProxy tagFileProxy,
      ILogger log, IDictionary<string, string> customHeaders)
    {
      try
      {
        var tRexResult = await tagFileProxy.SendTagFileDirect(compactionTagFileRequest,
          customHeaders).ConfigureAwait(false);

        return tRexResult as TagFileDirectSubmissionResult;
      }
      catch (Exception e)
      {
        log.LogError($"SendTagFileToTRex: returned exception: {e.Message}");
      }

      return TagFileDirectSubmissionResult.Create(
        new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprUnknown));
    }
  }
}

