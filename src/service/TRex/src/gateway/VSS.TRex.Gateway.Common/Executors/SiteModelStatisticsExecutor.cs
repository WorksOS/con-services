using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class SiteModelStatisticsExecutor : BaseExecutor
  {
    public SiteModelStatisticsExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SiteModelStatisticsExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ProjectStatisticsTRexRequest;
      if (request == null)
      {
        ThrowRequestTypeCastException<SiteModelStatisticsExecutor>();
        return null; // to keep compiler happy
      }

      var siteModel = GetSiteModel(request.ProjectUid);
      var extents = siteModel?.GetAdjustedDataModelSpatialExtents(request.ExcludedSurveyedSurfaceUids);

      var result = new ProjectStatisticsResult();
      if (extents != null)
        result.extents = new BoundingBox3DGrid(
          extents.MinX, extents.MinY, extents.MinZ,
          extents.MaxX, extents.MaxY, extents.MaxZ
        );

      var startEndDates = siteModel.GetDateRange();
      var format = "yyyy-MM-ddTHH-mm-ss.fffffff";
      result.startTime = DateTime.ParseExact(startEndDates.startUtc.ToString(format, CultureInfo.InvariantCulture), format, CultureInfo.InvariantCulture);
      result.endTime = DateTime.ParseExact(startEndDates.endUtc.ToString(format, CultureInfo.InvariantCulture), format, CultureInfo.InvariantCulture);

      result.cellSize = siteModel.Grid.CellSize;
      result.indexOriginOffset = (int)siteModel.Grid.IndexOriginOffset;
      return result;
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
