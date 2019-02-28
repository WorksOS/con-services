using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
#if RAPTOR
using BoundingExtents;
using SVOICStatistics;
#endif

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ProjectStatisticsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ProjectStatisticsMultiRequest;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_PROJECTSTATISTICS"))
#endif
      {
        var tRexRequest =
          new ProjectStatisticsTRexRequest(request.ProjectUid, request.ExcludedSurveyedSurfaceUids?.ToArray());
        return trexCompactionDataProxy.SendProjectStatisticsRequest(tRexRequest).Result;
      }
#if RAPTOR
      bool success = raptorClient.GetDataModelStatistics(
        request.ProjectId,
        RaptorConverters.convertSurveyedSurfaceExlusionList(request.ExcludedSurveyedSurfaceIds?.ToArray()),
        out var statistics);

      if (success)
        return ConvertProjectStatistics(statistics);
#endif

      throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
        "ProjectStatisticsExecutor: Unsupported path"));
    }

#if RAPTOR
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

    private static ProjectStatisticsResult ConvertProjectStatistics(TICDataModelStatistics statistics)
    {
      return new ProjectStatisticsResult
      {
        cellSize = statistics.CellSize,
        endTime = statistics.EndTime,
        startTime = statistics.StartTime,
        indexOriginOffset = statistics.IndexOriginOffset,
        extents = ConvertExtents(statistics.Extents)
      };
    }
#endif

  }
}

