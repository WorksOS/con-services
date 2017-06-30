using ASNode.ElevationStatistics.RPC;
using ASNodeDecls;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICOptionsDecls;
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
  public class ElevationStatisticsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public ElevationStatisticsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ElevationStatisticsExecutor()
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
      try
      {
        ElevationStatisticsRequest request = item as ElevationStatisticsRequest;
        TASNodeElevationStatisticsResult result = new TASNodeElevationStatisticsResult();

        TICFilterSettings Filter =
          RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.projectId);

        bool success = raptorClient.GetElevationStatistics(request.projectId ?? -1,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                                                                  ASNodeDecls.TASNodeCancellationDescriptorType.cdtElevationStatistics),
            Filter,
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            out result) == TASNodeErrorStatus.asneOK;

        if (success)
        {
          return ConvertResult(result);
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                  "Failed to calculate elevation statistics data"));
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