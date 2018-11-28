using System;
using ASNode.ElevationStatistics.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
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

    private BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(
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
      return ElevationStatisticsResult.CreateElevationStatisticsResult
      (
          ConvertExtents(result.BoundingExtents),
          result.MinElevation,
          result.MaxElevation,
          result.CoverageArea
      );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ElevationStatisticsRequest>(item);

      //new TASNodeElevationStatisticsResult();

      var Filter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);

      var raptorResult = raptorClient.GetElevationStatistics(request.ProjectId ?? -1,
                           ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                             TASNodeCancellationDescriptorType.cdtElevationStatistics),
                          Filter,
                          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
                          out var result);

      if (raptorResult == TASNodeErrorStatus.asneOK)
        return ConvertResult(result);

      throw CreateServiceException<ElevationStatisticsExecutor>((int)raptorResult);
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
