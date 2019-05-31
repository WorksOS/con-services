using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;

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
    [HttpPost]
    public ColorPaletteResult GetCCAColorPalette([FromBody] ColorPaletteRequest request)
    {
      Log.LogInformation($"{nameof(GetCCAColorPalette)}: {JsonConvert.SerializeObject(request)}");

      request.Validate();
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(nameof(GetCCAColorPalette), request.ProjectUid);
      GatewayHelper.ValidateMachines(new List<Guid?>{ request.AssetUid }, siteModel);
      return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<PaletteExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(request)) as ColorPaletteResult;
    }
  }
}
