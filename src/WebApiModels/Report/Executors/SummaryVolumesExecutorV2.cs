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
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  /// <summary>
  /// Summary volumes executor for use with API v2.
  /// </summary>
  public class SummaryVolumesExecutorV2 : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutorV2()
    {
      ProcessErrorCodes();
    }

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
      try
      {
        SummaryVolumesRequest request = item as SummaryVolumesRequest;

        if (request == null)
          ThrowRequestTypeCastException<SummaryVolumesRequest>();

        if (!bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_VOLUMES"), out var useTrexGateway))
          useTrexGateway = false;

        if (useTrexGateway)
        {
          var summaryVolumesRequest = new SummaryVolumesDataRequest(
            request.ProjectUid,
            request.BaseFilter,
            request.TopFilter,
            request.BaseDesignDescriptor.Uid,
            request.TopDesignDescriptor.Uid,
            request.VolumeCalcType);

          return trexCompactionDataProxy.SendSummaryVolumesRequest(summaryVolumesRequest, customHeaders).Result;
        }

        TASNodeSimpleVolumesResult result;

        var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilterId, request.BaseFilter, request.ProjectId);
        var topFilter = RaptorConverters.ConvertFilter(request.TopFilterId, request.TopFilter, request.ProjectId);
        var baseDesignDescriptor = RaptorConverters.DesignDescriptor(request.BaseDesignDescriptor);
        var topDesignDescriptor = RaptorConverters.DesignDescriptor(request.TopDesignDescriptor);

        TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.VolumeCalcType);

        // #68799 - Temporarily revert v2 executor behaviour to match that of v1 by adjusting filter dates on Filter to Filter calculations.
        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
        {
          RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
        }

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter,
          request.VolumeCalcType);
        // End #68799 fix.

        TASNodeErrorStatus raptorResult;

        if (request.CutTolerance != null && request.FillTolerance != null)
        {
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
              request.AdditionalSpatialFilter, request.ProjectId), (double) request.CutTolerance,
            (double) request.FillTolerance,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }
        else
        {
          raptorResult = this.raptorClient.GetSummaryVolumes(request.ProjectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(request.CallId ?? Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtVolumeSummary),
            volType,
            baseFilter,
            baseDesignDescriptor,
            topFilter,
            topDesignDescriptor,
            RaptorConverters.ConvertFilter(request.AdditionalSpatialFilterId,
              request.AdditionalSpatialFilter, request.ProjectId),
            RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone),
            out result);
        }

        if (raptorResult == TASNodeErrorStatus.asneOK)
          return ConvertResult(result);

        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult((int) raptorResult,
            $"Failed to get requested volumes summary data with error: {ContractExecutionStates.FirstNameWithOffset((int)raptorResult)}"));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}
