using ASNode.Volumes.RPC;
using ASNodeDecls;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICOptionsDecls;
using SVOICVolumeCalculationsDecls;
using System;
using System.Net;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  public class SummaryVolumesExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public SummaryVolumesExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
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

    private SummaryVolumesResult ConvertResult(TASNodeSimpleVolumesResult result)
    {
      return SummaryVolumesResult.CreateSummaryVolumesResult
      (
          ConvertExtents(result.BoundingExtents),
          result.Cut,
          result.Fill,
          result.TotalCoverageArea,
          result.CutArea,
          result.FillArea
      );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        SummaryVolumesRequest request = item as SummaryVolumesRequest;
        TASNodeSimpleVolumesResult result = new TASNodeSimpleVolumesResult();

        TICFilterSettings baseFilter =
          RaptorConverters.ConvertFilter(request.baseFilterID, request.baseFilter, request.projectId);
        TICFilterSettings topFilter =
          RaptorConverters.ConvertFilter(request.topFilterID, request.topFilter, request.projectId);
        TComputeICVolumesType volType = RaptorConverters.ConvertVolumesType(request.volumeCalcType);
        if (volType == TComputeICVolumesType.ic_cvtBetween2Filters)
          RaptorConverters.AdjustFilterToFilter(baseFilter, topFilter);

        RaptorConverters.reconcileTopFilterAndVolumeComputationMode(ref baseFilter, ref topFilter, request.volumeCalcType);

        bool success = false;
        if (request.CutTolerance != null && request.FillTolerance != null)
        {
          success = raptorClient.GetSummaryVolumes(request.projectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          RaptorConverters.DesignDescriptor(request.baseDesignDescriptor),
          topFilter,
          RaptorConverters.DesignDescriptor(request.topDesignDescriptor),
          RaptorConverters.ConvertFilter(request.additionalSpatialFilterID,
            request.additionalSpatialFilter, request.projectId, null, null), (double)request.CutTolerance,
          (double)request.FillTolerance,
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out result);
        }
        else
        {
          success = raptorClient.GetSummaryVolumes(request.projectId ?? -1,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
            TASNodeCancellationDescriptorType.cdtVolumeSummary),
          volType,
          baseFilter,
          RaptorConverters.DesignDescriptor(request.baseDesignDescriptor),
          topFilter,
          RaptorConverters.DesignDescriptor(request.topDesignDescriptor),
          RaptorConverters.ConvertFilter(request.additionalSpatialFilterID,
            request.additionalSpatialFilter, request.projectId, null, null),
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
          out result);
        }
        if (success)
        {
          return ConvertResult(result);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  "Failed to get requested volumes summary data"));
        }
      }
      finally
      {
      }

    }


    protected override void ProcessErrorCodes()
    {
    }
  }
}