using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Helpers;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace CCSS.TagFileSplitter.WebAPI.Common.Executors
{
  /// <summary>
  /// setup tasks for each service in targetServices
  ///   wait for all targets (within a reasonable response time)
  ///   return response from all targets, including their applicationIds 
  ///   TFH will call this endpoint, and process all target responses, archiving to appropriate directories in TCC
  /// </summary>
  public class AutoSubmissionExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CompactionTagFileRequest>(item, 68);
      
      var tasks = new List<Task<TargetServiceResponse>>();
      foreach (var targetService in TargetServices.Services)
        tasks.Add(TargetServiceHelper.SendTagFileTo3dPmService(request, ServiceResolution, GenericHttpProxy,
          targetService.ServiceName, targetService.TargetApiVersion, targetService.AutoRoute,
          Logger, CustomHeaders, TimeoutSeconds));
      await Task.WhenAll(tasks);

      var vssResults = tasks.Select(t => t.Result).ToArray();
      if (vssResults == null || vssResults.Count() != TargetServices.Services.Count)
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Incorrect number of result sets gathered. Expected: {TargetServices.Services.Count} got: {vssResults.Count()}"));

      var result = new TagFileSplitterAutoResponse(vssResults.ToList());
      return result;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
