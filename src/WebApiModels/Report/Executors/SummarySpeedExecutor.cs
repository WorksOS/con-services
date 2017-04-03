using System;
using System.Collections.Generic;
using System.Net;
using ASNode.SpeedSummary.RPC;
using ASNodeDecls;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
{
  /// <summary>
  /// Builds Summary speed report from Raptor
  /// </summary>
  public class SummarySpeedExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public SummarySpeedExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummarySpeedExecutor()
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

    private SummarySpeedResult ConvertResult(TASNodeSpeedSummaryResult result)
    {

      return SummarySpeedResult.CreateSummarySpeedResult
          (
              Math.Round(result.AboveTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.BelowTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.MatchTargetAreaPercent, 1, MidpointRounding.AwayFromZero),
              Math.Round(result.CovegareArea, 5)
          );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        SummarySpeedRequest request = item as SummarySpeedRequest;
        TASNodeSpeedSummaryResult result = new TASNodeSpeedSummaryResult();

        bool success = raptorClient.GetSummarySpeed(request.projectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                TASNodeCancellationDescriptorType.cdtVolumeSummary),
            RaptorConverters.ConvertFilter(request.filterId, request.filter, request.projectId, null, null,
                new List<long>()),
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
                  "Failed to get requested speed summary data"));
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