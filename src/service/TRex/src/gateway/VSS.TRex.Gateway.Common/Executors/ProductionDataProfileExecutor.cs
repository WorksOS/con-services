using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get production data profile.
  /// </summary>
  public class ProductionDataProfileExecutor : BaseExecutor
  {
    public ProductionDataProfileExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProductionDataProfileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ProductionDataProfileDataRequest request = item as ProductionDataProfileDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<ProductionDataProfileDataRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var baseFilter = ConvertFilter(request.BaseFilter, siteModel);

      ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = request.ProjectUid ?? Guid.Empty,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = request.PositionsAreGrid,
        Filters = new FilterSet(baseFilter),
        ReferenceDesign.DesignID = request.ReferenceDesignUid ?? Guid.Empty,
        ReferenceDesign.Offset = request.ReferenceDesignOffset ?? 0,
        StartPoint = new WGS84Point(request.StartX, request.StartY),
        EndPoint = new WGS84Point(request.EndX, request.EndY),
        ReturnAllPassesAndLayers = true
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var svRequest = new ProfileRequest_ApplicationService_ProfileCell();

      // var Response = svRequest.Execute(arg);
      ProfileRequestResponse<ProfileCell> response = svRequest.Execute(arg);

      if (response != null)
        return ConvertResult(response);

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
        "Failed to get requested Production Data Profile data"));
    }

    /// <summary>
    /// Converts SummaryVolumesProfileResponse data into SummaryVolumesProfileResult data.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private ProfileDataResult<ProfileCellData> ConvertResult(ProfileRequestResponse<ProfileCell> result)
    {
      List<ProfileCellData> profileCells = result.ProfileCells.Select(pc =>
          new ProfileCellData(
            pc.Station,
            pc.InterceptLength,
            pc.CellFirstElev,
            pc.CellLastElev,
            pc.CellLowestElev,
            pc.CellHighestElev,
            pc.CellFirstCompositeElev,
            pc.CellHighestCompositeElev,
            pc.CellLastCompositeElev,
            pc.CellLowestCompositeElev,
            pc.DesignElev,
            pc.CellCCV,
            pc.CellTargetCCV,
            pc.CellCCVElev,
            pc.CellPreviousMeasuredCCV,
            pc.CellPreviousMeasuredTargetCCV,
            pc.CellMDP,
            pc.CellTargetMDP,
            pc.CellMDPElev,
            pc.CellMaterialTemperature,
            pc.CellMaterialTemperatureWarnMin,
            pc.CellMaterialTemperatureWarnMax,
            pc.CellMaterialTemperatureElev,
            pc.CellTopLayerThickness,
            pc.TopLayerPassCount,
            pc.TopLayerPassCountTargetRangeMin,
            pc.TopLayerPassCountTargetRangeMax,
            pc.CellMinSpeed,
            pc.CellMaxSpeed))
        .ToList();

      return new ProfileDataResult<ProfileCellData>(result.GridDistanceBetweenProfilePoints, profileCells);
    }
  }
}
