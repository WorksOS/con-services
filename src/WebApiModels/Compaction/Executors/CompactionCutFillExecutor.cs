using System;
using System.Net;
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get cut-fill details.
  /// </summary>
  public class CompactionCutFillExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        CutFillDetailsRequest request = item as CutFillDetailsRequest;

        if (request == null)
          ThrowRequestTypeCastException<CutFillDetailsRequest>();

        if (!bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_CUTFILL"), out var useTrexGateway))
          useTrexGateway = false;
      
        if (useTrexGateway)
          return trexCompactionDataProxy.SendCutFillDetailsRequest(request, customHeaders).Result;

        var filter = RaptorConverters.ConvertFilter(null, request.Filter, request.ProjectId);
        var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);
        var liftBuildSettings =
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

        bool success = raptorClient.GetCutFillDetails(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtCutfillDetailed),
          new TCutFillSettings
          {
            Offsets = request.CutFillTolerances,
            DesignDescriptor = designDescriptor
          },
          filter,
          liftBuildSettings,
          out var cutFillDetails);

        if (success)
          return new CompactionCutFillDetailedResult(cutFillDetails.Percents);

        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to get requested cut-fill details data"));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
