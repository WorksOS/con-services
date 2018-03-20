using System.Net;
using BoundingExtents;
using SVOICStatistics;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Models.Report.Executors
{
  public class ProjectStatisticsExecutor : RequestExecutorContainer
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

    private ProjectStatisticsResult convertProjectStatistics(TICDataModelStatistics statistics)
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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ProjectStatisticsRequest request = item as ProjectStatisticsRequest;

      bool success = raptorClient.GetDataModelStatistics(
        request.projectId ?? -1,
        RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds),
        out var statistics);

      if (success)
      {
        return convertProjectStatistics(statistics);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Unable get data from Raptor."));
    }
  }
}