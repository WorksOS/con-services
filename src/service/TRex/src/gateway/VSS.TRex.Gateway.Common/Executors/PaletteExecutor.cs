using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Rendering.Palettes;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Processes the request to get a CCA palette
  /// </summary>
  public class PaletteExecutor : BaseExecutor
  {
    public PaletteExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler exceptionHandler)
      : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public PaletteExecutor()
    {

    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as ColorPaletteRequest;

      if (request == null)
        ThrowRequestTypeCastException<ColorPaletteRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);

      /*
      var cellDatumRequest = new CellDatumRequest_ApplicationService();
      var response = cellDatumRequest.Execute(new CellDatumRequestArgument_ApplicationService
      {
        ProjectID = siteModel.ID,
        Filters = new FilterSet(filter),
        Mode = request.DisplayMode,
        CoordsAreGrid = request.CoordsAreGrid,
        Point = request.CoordsAreGrid ? AutoMapperUtility.Automapper.Map<XYZ>(request.GridPoint) : AutoMapperUtility.Automapper.Map<XYZ>(request.LLPoint),
        ReferenceDesign = new DesignOffset(request.DesignUid ?? Guid.Empty, request.Offset ?? 0)
      });

      //      return new CompactionCellDatumResult(response.DisplayMode, response.ReturnCode, response.Value, response.TimeStampUTC, response.Northing, response.Easting);
      */

      //Default palette - TODO: see how Raptor gets from te machine
      var palette = new CCAPalette();
      return new ColorPaletteResult{Palettes = AutoMapperUtility.Automapper.Map<ColorPalette[]>(palette.PaletteTransitions)};
    }

  }
}
