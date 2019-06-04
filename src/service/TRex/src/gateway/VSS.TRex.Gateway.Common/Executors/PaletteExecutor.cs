using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Rendering;
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
      var request = item as CCAColorPaletteTrexRequest;

      if (request == null)
        ThrowRequestTypeCastException<CCAColorPaletteTrexRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);
      var ccaPalette = Utilities.ComputeCCAPalette(siteModel, filter.AttributeFilter, DisplayMode.CCA) as CCAPalette;
      return new CCAColorPaletteResult{Palettes = ccaPalette == null ? null : AutoMapperUtility.Automapper.Map<ColorPalette[]>(ccaPalette.PaletteTransitions)};
    }
  }
}
