using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TAGProcServiceDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting direct submitted TAG files to TRex.
  /// </summary>
  public class TagFileDirectSubmissionTRexExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CompactionTagFileRequest;

      // gobbles any exception
      var result = await TagFileHelper.SendTagFileToTRex(request,
        tRexTagFileProxy, log, customHeaders).ConfigureAwait(false);
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
