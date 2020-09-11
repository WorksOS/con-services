using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
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
      var request = item as ProgressiveSummaryVolumesDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<ProgressiveSummaryVolumesDataRequest>();

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
        VolumeType = ConvertVolumesType(request.VolumeCalcType),
        CutTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE,
        FillTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE
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
    /// Converts values of the VolumesType to ones of the VolumeComputationType.
    /// </summary>
    private VolumeComputationType ConvertVolumesType(VolumesType volumesType)
    {
      switch (volumesType)
      {
        case VolumesType.None: return VolumeComputationType.None;
        case VolumesType.AboveLevel: return VolumeComputationType.AboveLevel;
        case VolumesType.Between2Levels: return VolumeComputationType.Between2Levels;
        case VolumesType.AboveFilter: return VolumeComputationType.AboveFilter;
        case VolumesType.Between2Filters: return VolumeComputationType.Between2Filters;
        case VolumesType.BetweenFilterAndDesign: return VolumeComputationType.BetweenFilterAndDesign;
        case VolumesType.BetweenDesignAndFilter: return VolumeComputationType.BetweenDesignAndFilter;
        default: throw new ArgumentException($"Unknown VolumesType {Convert.ToInt16(volumesType)}");
      }
    }

    /// <summary>
    /// Converts SimpleVolumesResponse data into SummaryVolumesResult data.
    /// </summary>
    private ProgressiveSummaryVolumesResult ConvertResult(ProgressiveVolumesResponse result)
    {
      return new ProgressiveSummaryVolumesResult(
        result.Volumes.Select(x =>
          new ProgressiveSummaryVolumesResultItem(x.Date,
            SummaryVolumesResult.Create(ConvertExtents(x.Volume.BoundingExtentGrid),
              x.Volume.Cut ?? 0.0,
              x.Volume.Fill ?? 0.0,
              x.Volume.TotalCoverageArea ?? 0.0,
              x.Volume.CutArea ?? 0.0,
              x.Volume.FillArea ?? 0.0))).ToArray());
    }

    /// <summary>
    /// Converts BoundingWorldExtent3D data into BoundingBox3DGrid data.
    /// </summary>
    private BoundingBox3DGrid ConvertExtents(BoundingWorldExtent3D extents)
    {
      return new BoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ);
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
