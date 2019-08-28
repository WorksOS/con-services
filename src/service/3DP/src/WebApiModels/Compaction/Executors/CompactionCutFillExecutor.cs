using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get cut-fill details.
  /// </summary>
  public class CompactionCutFillExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionCutFillExecutor()
    {
      ProcessErrorCodes();
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CutFillDetailsRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_CUTFILL"))
        {
#endif
          var trexRequest = new TRexCutFillDetailsRequest(
            request.ProjectUid.Value,
            request.CutFillTolerances,
            request.Filter,
            request.DesignDescriptor,
            AutoMapperUtility.Automapper.Map<OverridingTargets>(request.LiftBuildSettings),
            AutoMapperUtility.Automapper.Map<LiftSettings>(request.LiftBuildSettings));
          return await trexCompactionDataProxy.SendDataPostRequest<CompactionCutFillDetailedResult, TRexCutFillDetailsRequest>(trexRequest, "/cutfill/details", customHeaders);
#if RAPTOR
        }

        var filter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);
        var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);
        var liftBuildSettings =
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

        var raptorResult = raptorClient.GetCutFillDetails(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtCutfillDetailed),
          new TCutFillSettings
          {
            Offsets = request.CutFillTolerances,
            DesignDescriptor = designDescriptor
          },
          filter,
          liftBuildSettings,
          out var cutFillDetails);

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return new CompactionCutFillDetailedResult(cutFillDetails.Percents);

        throw CreateServiceException<CompactionCutFillExecutor>((int)raptorResult);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

  }
}
