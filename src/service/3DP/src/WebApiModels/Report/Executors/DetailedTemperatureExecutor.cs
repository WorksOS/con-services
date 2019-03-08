using System;
using System.Linq;
#if RAPTOR
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class DetailedTemperatureExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<TemperatureDetailsRequest>(item);

      var temperatureTargets = request.Targets.Select(t => (int)t).ToArray(); // already converted to 10ths 
#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_TEMPERATURE"))
      {
#endif
        var temperatureDetailsRequest = new TemperatureDetailRequest(
          request.ProjectUid,
          request.Filter,
          temperatureTargets);

        var temperatureDetailsResult = trexCompactionDataProxy.SendDataPostRequest<TemperatureDetailResult, TemperatureDetailRequest>(temperatureDetailsRequest, "/temperature/details", customHeaders).Result as TemperatureDetailResult;

        return new CompactionTemperatureDetailResult(temperatureDetailsResult);
#if RAPTOR
      }

      var filter = RaptorConverters.ConvertFilter(request.Filter);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

      var raptorResult = raptorClient.GetTemperatureDetails(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtTemperatureDetailed),
        new TTemperatureDetailSettings
        {
          TemperatureList = temperatureTargets,
        },
        filter,
        liftBuildSettings,
        out var temperatureDetails);

      if (raptorResult == TASNodeErrorStatus.asneOK)
        return new CompactionTemperatureDetailResult(temperatureDetails.Percents);

      throw CreateServiceException<DetailedTemperatureExecutor>((int)raptorResult);
#endif
    }
  }
}
