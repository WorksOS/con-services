﻿using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
#if RAPTOR
using VSS.Productivity3D.Common.Algorithms;
#endif
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// GeoJSON controller.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class GeoJsonController : BaseController<GeoJsonController>
  {
    /// <inheritdoc />
    public GeoJsonController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager) :
      base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// 
    /// </summary>
    [HttpPost("api/v2/geojson/polyline/reducepoints")]
    public IActionResult PolylineReducePoints([FromBody] SmoothPolylineRequest requestDto)
    {
      Log.LogDebug($"{nameof(PolylineReducePoints)}: {requestDto}");

#if RAPTOR
      if (requestDto.MaxPoints > SmoothPolylineRequest.NOT_DEFINED && requestDto.MaxPoints < 3) return StatusCode((int)HttpStatusCode.BadRequest, new { Message = "Cannot reduce polyline to fewer than 3 points." });

      var fencePoints = DouglasPeucker.DouglasPeuckerByCount(requestDto.Coordinates, requestDto.MaxPoints);

      return StatusCode((int)HttpStatusCode.OK, new { coordinates = fencePoints } );
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
