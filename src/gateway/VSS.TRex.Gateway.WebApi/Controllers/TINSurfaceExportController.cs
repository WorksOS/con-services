using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  public class TTMAndMetaDatActioNResult// : IActionResult
  {
    public int a, b, c, d;

    public FileStreamResult theFile;

//    public byte[] data;

    //   public Task ExecuteResultAsync(ActionContext context)
//    {
    //      throw new NotImplementedException();
//      return null;
//    }
  }

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
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      Log.LogDebug("GetTINSurface: " + Request.QueryString);

      Log.LogDebug($"Accept header is {Request.Headers["Accept"]}");

      TINSurfaceExportRequest request = new TINSurfaceExportRequest
      {
        ProjectUid = projectUid,
        Tolerance = tolerance,
        Filter = FilterResult.CreateFilter(null) // Todo: Get the actual filter from the filterUid
      };

      request.Validate();

      var container = RequestExecutorContainer.Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler);

      var tinResult = WithServiceExceptionTryExecute(() => container.Process(request)) as TINSurfaceExportResult;

      return tinResult;
    }

    /// <summary>
    /// Web service end point controller for TIN surface export
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="tolerance"></param>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/v2/export/surface/ttm")]
    public IActionResult GetTINSurface2([FromQuery] Guid projectUid,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      Log.LogDebug("GetTINSurface: " + Request.QueryString);

      Log.LogDebug($"Accept header is {Request.Headers["Accept"]}");

      TINSurfaceExportRequest request = new TINSurfaceExportRequest
      {
        ProjectUid = projectUid,
        Tolerance = tolerance,
        Filter = FilterResult.CreateFilter(null) // Todo: Get the actual filter from the filterUid
      };

      request.Validate();

      var container = RequestExecutorContainer.Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler);

      var tinResult = WithServiceExceptionTryExecute(() => container.Process(request)) as TINSurfaceExportResult;

      if (Request.Headers["Accept"].Equals("application/ttm"))
        return new FileStreamResult(new MemoryStream(tinResult.TINData), "application/ttm");

      if (Request.Headers["Accept"].Equals("application/ttm-and-metadata"))
        return Ok(new TTMAndMetaDatActioNResult
        {
          a = tinResult.TINData.Length,

         theFile = new FileStreamResult(new MemoryStream(tinResult.TINData), "application/ttm")
        });

      return new FileStreamResult(new MemoryStream(tinResult.TINData), "application/ttm")
      {
        FileDownloadName = "DecimatedTIN.ttm"
      };
    }

  }
}
