using System;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;

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

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      SummaryVolumesDataRequest request = item as SummaryVolumesDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<SummaryVolumesDataRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var baseFilter = ConvertFilter(request.BaseFilter, siteModel);
      var topFilter = ConvertFilter(request.TopFilter, siteModel);
      var additionalSpatialFilter = ConvertFilter(request.AdditionalSpatialFilter, siteModel);

      SimpleVolumesRequest_ApplicationService summaryVolumesRequest = new SimpleVolumesRequest_ApplicationService();

      SimpleVolumesResponse simpleVolumesResponse = summaryVolumesRequest.Execute(new SimpleVolumesRequestArgument()
      {
        ProjectID = siteModel.ID,
        BaseFilter = baseFilter,
        TopFilter = topFilter,
        AdditionalSpatialFilter = additionalSpatialFilter,
        BaseDesignID = request.BaseDesignUid ?? Guid.Empty,
        TopDesignID = request.TopDesignUid ?? Guid.Empty,
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
      return BoundingBox3DGrid.CreatBoundingBox3DGrid(
        extents.MinX,
        extents.MinY,
        extents.MinZ,
        extents.MaxX,
        extents.MaxY,
        extents.MaxZ);
    }
  }
}
