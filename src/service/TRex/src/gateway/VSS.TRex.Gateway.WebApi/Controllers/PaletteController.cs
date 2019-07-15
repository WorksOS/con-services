using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting palettes
  /// </summary>
  [Route("api/v1")]
  public class PaletteController : BaseController
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public PaletteController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore) :
      base(loggerFactory, loggerFactory.CreateLogger<CellController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Gets the CCA color palette for the specified machine.
    /// </summary>
    [HttpPost("/ccacolors")]
    public CCAColorPaletteResult GetCCAColorPalette([FromBody] CCAColorPaletteTrexRequest request)
    {
      Log.LogInformation($"{nameof(GetCCAColorPalette)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      ValidateFilterMachines(nameof(GetCCAColorPalette), request.ProjectUid, request.Filter);
      return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<PaletteExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(request)) as CCAColorPaletteResult;
    }
  }
}
