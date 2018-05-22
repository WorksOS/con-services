using System;
using System.Net;
using ASNode.Volumes.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class SummaryVolumesExecutor : RequestExecutorContainer
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

      var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilterId, request.BaseFilter, request.ProjectId);
      var topFilter = RaptorConverters.ConvertFilter(request.TopFilterId, request.TopFilter, request.ProjectId);
      var baseDesignDescriptor = RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor);
      var topDesignDescriptor = RaptorConverters.DesignDescriptor(request.TopDesignDescriptor);

      TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
        RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
      }

      RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter, request.VolumeCalcType);

      bool success;

      if (request.CutTolerance != null && request.FillTolerance != null)
      {
        success = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          baseDesignDescriptor,
          topFilter,
          topDesignDescriptor,
          RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
            request.AdditionalSpatialFilter, request.ProjectId), (double)request.CutTolerance,
          (double)request.FillTolerance,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
          out result);
      }
      else
      {
        success = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor),
          topFilter,
          RaptorConverters.DesignDescriptor(request.TopDesignDescriptor),
          RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
            request.AdditionalSpatialFilter, request.ProjectId),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
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
