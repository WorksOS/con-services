using System;
using ASNode.ThicknessSummary.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Builds Summary thickness report from Raptor
  /// </summary>
  public class SummaryThicknessExecutor : RequestExecutorContainer
  {
    private BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(
          extents.MinX,
          extents.MinY,
          extents.MinZ,
          extents.MaxX,
          extents.MaxY,
          extents.MaxZ);
    }

    private SummaryThicknessResult ConvertResult(TASNodeThicknessSummaryResult result)
    {

      return SummaryThicknessResult.Create
          (
              ConvertExtents(result.BoundingExtents),
              Math.Round(result.AboveTargetArea, 5),
              Math.Round(result.BelowTargetArea, 5),
              Math.Round(result.MatchTargetArea, 5),
              Math.Round(result.NoCovegareArea, 5)
          );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<SummaryParametersBase>(item);

      bool success = raptorClient.GetSummaryThickness(request.ProjectId ?? -1,
        ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
          TASNodeCancellationDescriptorType.cdtVolumeSummary),
        RaptorConverters.ConvertFilter(request.BaseFilterId, request.BaseFilter, request.ProjectId, null, null),
        RaptorConverters.ConvertFilter(request.TopFilterId, request.TopFilter, request.ProjectId, null, null),
        RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
          request.AdditionalSpatialFilter, request.ProjectId, null, null),
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
        out var result);

      if (success)
        return ConvertResult(result);

      throw CreateServiceException<SummaryThicknessExecutor>();
    }
  }
}
