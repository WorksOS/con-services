using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Helpers;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace CCSS.TagFileSplitter.WebAPI.Common.Executors
{
  /// <summary>
  /// setup tasks for appropriate services in _targetServices.
  ///    TMC request should go ONLY to Productivity3D service (not VSS)
  ///    wait for all targets (within a reasonable response time)
  ///    return response from all targets, including their applicationIds 
  /// TFH to be changed to call this, and process all target responses to enable it to archive to appropriate directories in TCC
  /// </summary>
  public class DirectSubmissionExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionTagFileRequest>(item, 68);

      var TMCApplicationName = ConfigStore.GetValueString("TAGFILE_TMC_APPLICATION_NAME", string.Empty);
      var isTmcDevice = string.Compare(UserEmailAddress, TMCApplicationName, StringComparison.OrdinalIgnoreCase) == 0;
      Logger.LogDebug($"{nameof(DirectSubmissionExecutor)}: request {request}, TMCApplicationName: {TMCApplicationName}");

      var tasks = new List<Task<TargetServiceResponse>>();
      foreach (var targetService in TargetServices.Services)
      {
        if (!isTmcDevice || (isTmcDevice && targetService.ServiceName == ServiceNameConstants.PRODUCTIVITY3D_SERVICE))
          tasks.Add(TargetServiceHelper.SendTagFileTo3dPmService(request, ServiceResolution, GenericHttpProxy,
            targetService.ServiceName, targetService.TargetApiVersion, targetService.DirectRoute,
            Logger, CustomHeaders, TimeoutSeconds));
      }
      await Task.WhenAll(tasks);

      var vssResults = tasks.Select(t => t.Result).ToArray();
      if (vssResults == null || (isTmcDevice && vssResults.Length > 1) || (!isTmcDevice && vssResults.Length != TargetServices.Services.Count))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Incorrect number of result sets gathered. Expected: {TargetServices.Services.Count} got: {vssResults.Count()}"));

      // return the first !success response if available
     Logger.LogDebug($":{nameof(DirectSubmissionExecutor)} fullResults: {vssResults}");

      var result = new ContractExecutionResult();
      var failedResult = vssResults.First(r => r.StatusCode != HttpStatusCode.OK);
      result = failedResult != null
        ? new ContractExecutionResult(failedResult.Code, failedResult.Message)
        : new ContractExecutionResult(vssResults[0].Code, vssResults[0].Message);
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
