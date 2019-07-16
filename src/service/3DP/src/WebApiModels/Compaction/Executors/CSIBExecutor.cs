using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
#endif
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Executor for processing DXF linework files.
  /// </summary>
  public class CSIBExecutor : RequestExecutorContainer
  {
    public CSIBExecutor()
    { }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      return await RequestCSIBForProject(CastRequestObjectTo<ProjectID>(item));
    }

    private async Task<ContractExecutionResult> RequestCSIBForProject(ProjectID request)
    {
#if RAPTOR
      if (configStore.GetValueBool("ENABLE_TREX_GATEWAY_CS") ?? false)
      {
#endif
        var siteModelId = request.ProjectUid.ToString();

        var returnedResult = await trexCompactionDataProxy.SendDataGetRequest<CSIBResult>(siteModelId, $"/projects/{siteModelId}/csib", customHeaders);

        return returnedResult.CSIB == string.Empty
          ? new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, $"{nameof(RequestCSIBForProject)}: result: {returnedResult}")
          : returnedResult;

#if RAPTOR
      }

      log.LogDebug($"{nameof(GetType)}::{nameof(RequestCSIBForProject)}() : {JsonConvert.SerializeObject(request)}");

      var returnResult = raptorClient.GetCSIBFile(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out var csibFileStream);

      log.LogInformation($"{nameof(RequestCSIBForProject)}: result: {returnResult}");

      return returnResult != TASNodeErrorStatus.asneOK
        ? new ContractExecutionResult((int) returnResult, $"{nameof(RequestCSIBForProject)}: result: {returnResult}")
        : new CSIBResult(Convert.ToBase64String(csibFileStream.ToArray()));
#endif
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
