using ASNode.Volumes.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Summary volumes executor for use with API v2.
  /// </summary>
  public class SummaryVolumesExecutorV2 : RequestExecutorContainer
  {
    private static BoundingBox3DGrid ConvertExtents(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ);
    }

    private static SummaryVolumesResult ConvertResult(TASNodeSimpleVolumesResult result)
    {
      return SummaryVolumesResult.Create(
        ConvertExtents(result.BoundingExtents),
        result.Cut,
        result.Fill,
        result.TotalCoverageArea,
        result.CutArea,
        result.FillArea);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      SummaryVolumesRequest request = item as SummaryVolumesRequest;
      if (request == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "SummaryVolumesRequest cast failed."));
      }

      TASNodeSimpleVolumesResult result;

      var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilterId, request.BaseFilter, request.projectId);
      var topFilter = RaptorConverters.ConvertFilter(request.TopFilterId, request.TopFilter, request.projectId);
      var baseDesignDescriptor = RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor);
      var topDesignDescriptor = RaptorConverters.DesignDescriptor(request.TopDesignDescriptor);

      TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

      bool success;

      if (request.CutTolerance != null && request.FillTolerance != null)
      {
        success = this.raptorClient.GetSummaryVolumes(request.projectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          baseDesignDescriptor,
          topFilter,
          topDesignDescriptor,
          RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
            request.AdditionalSpatialFilter, request.projectId), (double)request.CutTolerance,
          (double)request.FillTolerance,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out result);
      }
      else
      {
        success = this.raptorClient.GetSummaryVolumes(request.projectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          baseDesignDescriptor,
          topFilter,
          topDesignDescriptor,
          RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
            request.AdditionalSpatialFilter, request.projectId),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out result);
      }

      if (success)
      {
        return ConvertResult(result);
      }

      throw new ServiceException(
        HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Failed to get requested volumes summary data"));
    }
  }
}