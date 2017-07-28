using BoundingExtents;
using Microsoft.Extensions.Logging;
using SVOICStatistics;
using System.Collections.Generic;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Executors
{
  public class ProjectStatisticsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public ProjectStatisticsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProjectStatisticsExecutor()
    {
    }

    private static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(long[] exclusions)
    {
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Length];
      for (int i = 0; i < exclusions.Length; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }

    private static TSurveyedSurfaceID[] convertSurveyedSurfaceExlusionList(List<long> exclusions)
    {
      TSurveyedSurfaceID[] result = new TSurveyedSurfaceID[exclusions.Count];
      for (int i = 0; i < exclusions.Count; i++)
        result[i].SurveyedSurfaceID = exclusions[i];

      return result;
    }

    private BoundingBox3DGrid convertExtents(T3DBoundingWorldExtent extents)
    {
      return BoundingBox3DGrid.CreatBoundingBox3DGrid
             (
                     extents.MinX,
                     extents.MinY,
                     extents.MinZ,
                     extents.MaxX,
                     extents.MaxY,
                     extents.MaxZ
             );
    }

    private ProjectStatisticsResult convertProjectStatistics(TICDataModelStatistics statistics)
    {
      return new ProjectStatisticsResult
      {
        cellSize = statistics.CellSize,
        endTime = statistics.EndTime,
        startTime = statistics.StartTime,
        indexOriginOffset = statistics.IndexOriginOffset,
        extents = convertExtents(statistics.Extents)
      };
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ProjectStatisticsRequest request = item as ProjectStatisticsRequest;
      TICDataModelStatistics statistics;
      bool success = raptorClient.GetDataModelStatistics(request.projectId ?? -1,
              RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds), out statistics);
      if (success)
      {
        return convertProjectStatistics(statistics);
      }

      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
          "Unable get data from Raptor."));
    }

    protected override void ProcessErrorCodes()
    { }
  }
}