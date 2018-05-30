using System;
using System.Net;
using ASNode.ElevationStatistics.RPC;
using ASNodeDecls;
using BoundingExtents;
using SVOICFilterSettings;
using SVOICOptionsDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ElevationStatisticsExecutor : RequestExecutorContainer
  {
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
      ElevationStatisticsRequest request = item as ElevationStatisticsRequest;
      TASNodeElevationStatisticsResult result = new TASNodeElevationStatisticsResult();

      TICFilterSettings Filter =
        RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);

      bool success = raptorClient.GetElevationStatistics(request.ProjectId ?? -1,
                       ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor((Guid)(request.callId ?? Guid.NewGuid()), 0,
                         ASNodeDecls.TASNodeCancellationDescriptorType.cdtElevationStatistics),
                       Filter,
                       RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
                       out result) == TASNodeErrorStatus.asneOK;

      if (success)
      {
        return ConvertResult(result);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Failed to calculate elevation statistics data"));
    }
  }
}
