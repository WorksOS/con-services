using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
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

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as ProductionDataProfileDataRequest;

      if (request == null)
        ThrowRequestTypeCastException<ProductionDataProfileDataRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      var baseFilter = ConvertFilter(request.Filter, siteModel);
      var referenceDesign = new DesignOffset(request.ReferenceDesignUid ?? Guid.Empty, request.ReferenceDesignOffset ?? 0);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = request.ProjectUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = request.PositionsAreGrid,
        Filters = new FilterSet(baseFilter),
        ReferenceDesign = referenceDesign,
        StartPoint = new WGS84Point(request.StartX, request.StartY),
        EndPoint = new WGS84Point(request.EndX, request.EndY),
        ReturnAllPassesAndLayers = true,
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides),
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType)
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var svRequest = new ProfileRequest_ApplicationService_ProfileCell();

      var response = await svRequest.ExecuteAsync(arg);

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
      var profileCells = result.ProfileCells.Select(pc =>
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

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
