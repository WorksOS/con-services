using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get Summary Volumes statistics.
  /// </summary>
  public class SummaryVolumesExecutor : BaseExecutor
  {
    public SummaryVolumesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public SummaryVolumesExecutor()
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as SummaryVolumesDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<SummaryVolumesDataRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var baseFilter = ConvertFilter(request.BaseFilter, siteModel);
      var topFilter = ConvertFilter(request.TopFilter, siteModel);
      var additionalSpatialFilter = ConvertFilter(request.AdditionalSpatialFilter, siteModel);

      var summaryVolumesRequest = new SimpleVolumesRequest_ApplicationService();

      var simpleVolumesResponse = await summaryVolumesRequest.ExecuteAsync(new SimpleVolumesRequestArgument()
      {
        ProjectID = siteModel.ID,
        BaseFilter = baseFilter,
        TopFilter = topFilter,
        AdditionalSpatialFilter = additionalSpatialFilter,
        BaseDesign = new DesignOffset(request.BaseDesignUid ?? Guid.Empty, request.BaseDesignOffset ?? 0),
        TopDesign = new DesignOffset(request.TopDesignUid ?? Guid.Empty, request.TopDesignOffset ?? 0),
        VolumeType = ConvertVolumesType(request.VolumeCalcType),
        CutTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE,
        FillTolerance = request.CutTolerance ?? VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE
      });

      if (simpleVolumesResponse != null)
        return ConvertResult(simpleVolumesResponse);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Summary Volumes data"));
    }

    /// <summary>
    /// Converts values of the VolumesType to ones of the VolumeComputationType.
    /// </summary>
    /// <param name="volumesType"></param>
    /// <returns></returns>
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
    /// <param name="result"></param>
    /// <returns></returns>
    private SummaryVolumesResult ConvertResult(SimpleVolumesResponse result)
    {
      return SummaryVolumesResult.Create(
        ConvertExtents(result.BoundingExtentGrid),
        result.Cut ?? 0.0,
        result.Fill ?? 0.0,
        result.TotalCoverageArea ?? 0.0,
        result.CutArea ?? 0.0,
        result.FillArea ?? 0.0);
    }

    /// <summary>
    /// Converts BoundingWorldExtent3D data into BoundingBox3DGrid data.
    /// </summary>
    /// <param name="extents"></param>
    /// <returns></returns>
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
