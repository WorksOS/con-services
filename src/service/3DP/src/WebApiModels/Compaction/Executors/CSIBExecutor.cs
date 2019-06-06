using System;
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      return RequestCSIBForProject(CastRequestObjectTo<ProjectID>(item));
    }

    private ContractExecutionResult RequestCSIBForProject(ProjectID request)
    {
#if RAPTOR
      if (!bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_CS"), out var useTrexGateway))
        useTrexGateway = false;

      if (useTrexGateway)
      {
#endif
        var siteModelId = request.ProjectUid.ToString();

        var returnedResult = trexCompactionDataProxy.SendDataGetRequest<CSIBResult>(siteModelId, $"/projects/{siteModelId}/csib", customHeaders).Result;

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
  }
}
