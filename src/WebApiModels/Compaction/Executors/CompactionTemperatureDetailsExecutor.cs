using System;
using System.Linq;
using System.Net;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class CompactionTemperatureDetailsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      TemperatureDetailsRequest request = item as TemperatureDetailsRequest;

      var filter = RaptorConverters.ConvertFilter(null, request.Filter, request.ProjectId);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

      bool success = raptorClient.GetTemperatureDetails(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtTemperatureDetailed),
        new TTemperatureDetailSettings
        {
          TemperatureList = request.Targets.Select(t => (int)(t * 10)).ToArray(),//Raptor expects 10ths of degrees
        },
        filter,
        liftBuildSettings,
        out var temperatureDetails) == TASNodeErrorStatus.asneOK;
  

      if (success)
      {
        result = new CompactionTemperatureDetailResult(temperatureDetails.Percents);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested temperature details data"));
      }

      return result;
    }
  }
}
