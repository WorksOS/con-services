using System;
using System.Linq;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
      var request = item as TemperatureDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<TemperatureDetailsRequest>();

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_TEMPERATURE"), out var useTrexGateway);

      var temperatureTargets = request.Targets.Select(t => (int) t).ToArray(); // already converted to 10ths 

      if (useTrexGateway)
      {
        var temperatureDetailsRequest = new TemperatureDetailRequest(
          request.ProjectUid,
          request.Filter,
          temperatureTargets);

        var temperatureDetailsResult = trexCompactionDataProxy.SendTemperatureDetailsRequest(temperatureDetailsRequest, customHeaders).Result as TemperatureDetailResult;

        return new CompactionTemperatureDetailResult(temperatureDetailsResult);
      }

      var filter = RaptorConverters.ConvertFilter(null, request.Filter, request.ProjectId);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

      var raptorResult = raptorClient.GetTemperatureDetails(request.ProjectId ?? -1,
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
    }
  }
}
