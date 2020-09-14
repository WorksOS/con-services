using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class ProgressiveSummaryVolumesExecutor : BaseExecutor
  {
    public ProgressiveSummaryVolumesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
                                             IServiceExceptionHandler exceptionHandler)
    : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProgressiveSummaryVolumesExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProgressiveSummaryVolumesDataRequest>(item);// as ProgressiveSummaryVolumesDataRequest;

      // ReSharper disable once PossibleNullReferenceException
      var siteModel = GetSiteModel(request.ProjectUid);

      var filter = ConvertFilter(request.Filter, siteModel);
      var additionalSpatialFilter = ConvertFilter(request.AdditionalSpatialFilter, siteModel);

      var tRexRequest = new ProgressiveVolumesRequest_ApplicationService();

      var response = await tRexRequest.ExecuteAsync(new ProgressiveVolumesRequestArgument
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter), // Progressive volumes use a single filter derived from the supplied base filter
        AdditionalSpatialFilter = additionalSpatialFilter,
        BaseDesign = new DesignOffset(request.BaseDesignUid ?? Guid.Empty, request.BaseDesignOffset ?? 0),
        TopDesign = new DesignOffset(request.TopDesignUid ?? Guid.Empty, request.TopDesignOffset ?? 0),
        VolumeType = ConvertVolumesHelper.ConvertVolumesType(request.VolumeCalcType),
        CutTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE,
        FillTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE,
        StartDate = request.StartDate,
        EndDate = request.EndDate,
        Interval = TimeSpan.FromSeconds(request.IntervalSeconds)
      });

      if (response != null)
      {
        log.LogInformation($"Volume response is {JsonConvert.SerializeObject(response)}");
        return ConvertResult(response);
      }

      log.LogWarning("Volume response is null");
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Summary Volumes data"));
    }

    /// <summary>
    /// Converts ProgressiveVolumesResponse data into SummaryVolumesResult data.
    /// </summary>
    private ProgressiveSummaryVolumesResult ConvertResult(ProgressiveVolumesResponse result)
    {
      return ProgressiveSummaryVolumesResult.Create(
        result.Volumes.Select(x =>
          ProgressiveSummaryVolumesResultItem.Create(x.Date,
            SummaryVolumesResult.Create(BoundingBox3DGridHelper.ConvertExtents(x.Volume.BoundingExtentGrid),
              x.Volume.Cut ?? 0.0,
              x.Volume.Fill ?? 0.0,
              x.Volume.TotalCoverageArea ?? 0.0,
              x.Volume.CutArea ?? 0.0,
              x.Volume.FillArea ?? 0.0))).ToArray());
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
