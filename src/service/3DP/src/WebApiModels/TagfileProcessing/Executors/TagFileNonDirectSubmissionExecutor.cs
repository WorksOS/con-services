using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting TAG files to Raptor.
  /// </summary>
  public class TagFileNonDirectSubmissionExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build.
    /// </summary>
    public TagFileNonDirectSubmissionExecutor()
    {
      ProcessErrorCodes();
    }

    protected sealed override void ProcessErrorCodes()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionTagFileRequestExtended>(item);

      request.Validate();
      var returnResult = await TagFileHelper.SendTagFileToTRex(request, tRexTagFileProxy, log, customHeaders);

      log.LogInformation($"{nameof(TagFileNonDirectSubmissionExecutor)} completed: filename {request.FileName}  result {JsonConvert.SerializeObject(returnResult)}");
      if (returnResult.Code != 0)
        log.LogDebug($"{nameof(TagFileNonDirectSubmissionExecutor)}: Failed to import tagfile '{request.FileName}', {returnResult.Message}");
      
      return returnResult;
    }
  }
}
