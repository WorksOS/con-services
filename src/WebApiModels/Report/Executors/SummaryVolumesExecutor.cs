using System;
using System.Net;
using ASNode.Volumes.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

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
      return SummaryVolumesResult.CreateSummaryVolumesResult(
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

      TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

      if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
      {
           RaptorConverters.AdjustFilterToFilter(baseFilter, topFilter);
      }

      RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter, request.VolumeCalcType);

      bool success;

      if (request.CutTolerance != null && request.FillTolerance != null)
      {
        success = this.raptorClient.GetSummaryVolumes(request.projectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor),
          topFilter,
          RaptorConverters.DesignDescriptor(request.TopDesignDescriptor),
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
          RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor),
          topFilter,
          RaptorConverters.DesignDescriptor(request.TopDesignDescriptor),
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