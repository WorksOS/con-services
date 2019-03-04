using System;
#if RAPTOR
using ASNodeDecls;
using SVOICOptionsDecls;
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<CutFillDetailsRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_CUTFILL"))
        {
#endif
          return trexCompactionDataProxy.SendDataPostRequest<CompactionCutFillDetailedResult, CutFillDetailsRequest>(request, "/cutfill/details", customHeaders).Result;
#if RAPTOR
        }

        var filter = RaptorConverters.ConvertFilter(request.Filter);
        var designDescriptor = RaptorConverters.DesignDescriptor(request.DesignDescriptor);
        var liftBuildSettings =
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);

        var raptorResult = raptorClient.GetCutFillDetails(request.ProjectId ?? -1,
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
