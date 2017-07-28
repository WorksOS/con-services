using ASNode.ThicknessSummary.RPC;
using ASNodeDecls;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using System;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  /// <summary>
  /// Builds Summary thickness report from Raptor
  /// </summary>
  public class SummaryThicknessExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public SummaryThicknessExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryThicknessExecutor()
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

    private SummaryThicknessResult ConvertResult(TASNodeThicknessSummaryResult result)
    {

      return SummaryThicknessResult.CreateSummaryThicknessResult
          (
              ConvertExtents(result.BoundingExtents),
              Math.Round(result.AboveTargetArea,5) ,
              Math.Round(result.BelowTargetArea,5) ,
              Math.Round(result.MatchTargetArea,5) ,
              Math.Round(result.NoCovegareArea, 5) 
          );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        SummaryParametersBase request = item as SummaryParametersBase;
        TASNodeThicknessSummaryResult result = new TASNodeThicknessSummaryResult();

        bool success = raptorClient.GetSummaryThickness(request.projectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid) (request.callId ?? Guid.NewGuid()), 0,
                TASNodeCancellationDescriptorType.cdtVolumeSummary),
            RaptorConverters.ConvertFilter(request.baseFilterID, request.baseFilter, request.projectId, null, null),
            RaptorConverters.ConvertFilter(request.topFilterID, request.topFilter, request.projectId, null, null),
            RaptorConverters.ConvertFilter(request.additionalSpatialFilterID,
                request.additionalSpatialFilter, request.projectId, null, null),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            out result);
        if (success)
        {
          return ConvertResult(result);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  "Failed to get requested thickness summary data"));
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