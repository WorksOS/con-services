using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Patches.Requestors;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// The controller for generating TIN surfaces decimatied from elevation data
  /// </summary>
  public class TINSurfaceExportController : BaseController
  {
    private ITINSurfaceExportRequestor tINSurfaceExportRequestor;

    /// <summary>
    /// Constructor for TIN surface export controller
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    /// <param name="tINSurfaceExportRequestor"></param>
    public TINSurfaceExportController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore, ITINSurfaceExportRequestor tINSurfaceExportRequestor)
      : base(loggerFactory, loggerFactory.CreateLogger<TINSurfaceExportController>(), exceptionHandler, configStore)
    {
      this.tINSurfaceExportRequestor = tINSurfaceExportRequestor;
    }


    /// <summary>
    /// Web service end point controller for TIN surface export
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="tolerance"></param>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/v1/export/surface/ttm")]
    public TINSurfaceExportResult GetTINSurface([FromQuery] Guid projectUid,
      [FromQuery] double ? tolerance,
      [FromQuery] Guid ? filterUid)
    {
      Log.LogDebug("GetTINSurface: " + Request.QueryString);

      TINSurfaceExportRequest request = new TINSurfaceExportRequest
      {
        ProjectUid = projectUid,
        Tolerance = tolerance,
        Filter = FilterResult.CreateFilter(null) // Todo: Get the actual filter from the filterUid
      };

      // request.Validate();

      var container = RequestExecutorContainer.Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, null);

      container.tINSurfaceExportRequestor = tINSurfaceExportRequestor;

      var tinResult = WithServiceExceptionTryExecute(() => container.Process(request)) as TINSurfaceExportResult;

      return tinResult;
    }
  }
}
