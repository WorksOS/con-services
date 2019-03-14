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
      log.LogDebug($"{nameof(GetType)}::{nameof(RequestCSIBForProject)}() : {JsonConvert.SerializeObject(request)}");

      var returnResult = raptorClient.GetCSIBFile(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out var csibFileStream);

      log.LogInformation($"{nameof(RequestCSIBForProject)}: result: {returnResult}");

      return returnResult != TASNodeErrorStatus.asneOK
        ? new ContractExecutionResult((int)returnResult, $"{nameof(RequestCSIBForProject)}: result: {returnResult}")
        : new ContractExecutionResult((int)returnResult, Convert.ToBase64String(csibFileStream.ToArray()));
#else
      return null;
#endif
    }
  }
}
