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
      var request = item as TemperatureDetailsRequest;

      if (request == null)
        ThrowRequestTypeCastException<TemperatureDetailsRequest>();

      var filter = RaptorConverters.ConvertFilter(null, request.Filter, request.ProjectId);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

      var raptorResult = raptorClient.GetTemperatureDetails(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtTemperatureDetailed),
        new TTemperatureDetailSettings
        {
          TemperatureList = request.Targets.Select(t => (int)t).ToArray(),//already converted to 10ths 
        },
        filter,
        liftBuildSettings,
        out var temperatureDetails);
  
      if (raptorResult == TASNodeErrorStatus.asneOK)
        return new CompactionTemperatureDetailResult(temperatureDetails.Percents);

      throw CreateServiceException<CompactionTemperatureDetailsExecutor>((int)raptorResult);
    }
  }
}
