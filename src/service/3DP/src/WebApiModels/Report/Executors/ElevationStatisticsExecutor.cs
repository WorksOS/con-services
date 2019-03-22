using System;
using System.Threading.Tasks;
#if RAPTOR
using ASNode.ElevationStatistics.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
#endif
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ElevationStatisticsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ElevationStatisticsExecutor()
    {
      ProcessErrorCodes();
    }
#if RAPTOR
    private BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return new BoundingBox3DGrid(
              extents.MinX,
              extents.MinY,
              extents.MinZ,
              extents.MaxX,
              extents.MaxY,
              extents.MaxZ
              );
    }

    private ElevationStatisticsResult ConvertResult(TASNodeElevationStatisticsResult result)
    {
      return new ElevationStatisticsResult
      (
          ConvertExtents(result.BoundingExtents),
          result.MinElevation,
          result.MaxElevation,
          result.CoverageArea
      );
    }
#endif

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ElevationStatisticsRequest>(item);
#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_ELEVATION") || UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
#endif
        return await trexCompactionDataProxy.SendDataPostRequest<ElevationStatisticsResult, ElevationStatisticsRequest>(request, "/elevationstatistics", customHeaders);
#if RAPTOR
      //new TASNodeElevationStatisticsResult();

      var Filter = RaptorConverters.ConvertFilter(request.Filter);

      var raptorResult = raptorClient.GetElevationStatistics(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
                           ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                             TASNodeCancellationDescriptorType.cdtElevationStatistics),
                          Filter,
                          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
                          out var result);

      if (raptorResult == TASNodeErrorStatus.asneOK)
        return ConvertResult(result);

      throw CreateServiceException<ElevationStatisticsExecutor>((int)raptorResult);
#endif
    }

    protected sealed override void ProcessErrorCodes()
    {
#if RAPTOR
      RaptorResult.AddErrorMessages(ContractExecutionStates);
#endif
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
