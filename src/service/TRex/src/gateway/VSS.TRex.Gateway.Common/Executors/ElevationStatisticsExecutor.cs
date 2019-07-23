using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using ElevationDataResult = VSS.Productivity3D.Models.ResultHandling.ElevationStatisticsResult;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class ElevationStatisticsExecutor : BaseExecutor
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ElevationStatisticsExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as ElevationDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<ElevationStatisticsExecutor>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var operation = new ElevationStatisticsOperation();
      var elevationStatisticsResult = await operation.ExecuteAsync(new ElevationStatisticsArgument()
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet { Filters = new[] { new CombinedFilter() } }
      });

      if (elevationStatisticsResult != null)
      {
        if (elevationStatisticsResult.ResultStatus == RequestErrorStatus.OK)
          return ConvertResult(elevationStatisticsResult);

        throw CreateServiceException<ElevationStatisticsExecutor>(elevationStatisticsResult.ResultStatus);
      }

      throw CreateServiceException<ElevationStatisticsExecutor>();
    }

    private ElevationDataResult ConvertResult(Analytics.ElevationStatistics.ElevationStatisticsResult result)
    {
      return new ElevationDataResult(
        new BoundingBox3DGrid(
          result.BoundingExtents.MinX, 
          result.BoundingExtents.MinY, 
          result.BoundingExtents.MinZ,
          result.BoundingExtents.MaxX,
          result.BoundingExtents.MaxY,
          result.BoundingExtents.MaxZ),
        result.MinElevation, 
        result.MaxElevation,
        result.CoverageArea);
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
