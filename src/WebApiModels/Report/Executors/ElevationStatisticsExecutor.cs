using BoundingExtents;
using System;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using ASNode.ElevationStatistics.RPC;
using SVOICFilterSettings;
using SVOICOptionsDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;
using ASNodeDecls;
using Microsoft.Extensions.Logging;

namespace VSS.Raptor.Service.WebApiModels.Report.Executors
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
          throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
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