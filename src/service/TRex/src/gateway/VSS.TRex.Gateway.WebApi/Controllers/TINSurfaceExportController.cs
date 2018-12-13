using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;
using FileSystem = System.IO.File;


namespace VSS.TRex.Gateway.WebApi.Controllers
{
  public class TTMAndMetaDatActioNResult // : IActionResult
  {
    public long a, b, c, d;

    public FileStreamResult theFile;
  }

  /// <summary>
  /// The controller for generating TIN surfaces decimated from elevation data
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
    /// <param name="compactionExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/surface/ttm")]
    public CompactionExportResult PostTINSurface([FromBody] CompactionExportRequest compactionExportRequest)
    {
      Log.LogInformation($"{nameof(PostTINSurface)}: {Request.QueryString}");

      Log.LogDebug($"Accept header is {Request.Headers["Accept"]}");

      compactionExportRequest.Validate();

      var tinResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(compactionExportRequest) as TINSurfaceExportResult);

      const string TTM_EXTENSION = ".ttm";
      const string ZIP_EXTENSION = ".zip";

      var fullFileName = BuildTINFilePath(compactionExportRequest.FileName, ZIP_EXTENSION);

      if (FileSystem.Exists(fullFileName))
        FileSystem.Delete(fullFileName);

      using (var zipFile = ZipFile.Open(fullFileName, ZipArchiveMode.Create))
      {
        var entry = zipFile.CreateEntry(compactionExportRequest.FileName + TTM_EXTENSION);
        using (var stream = entry.Open())
          new MemoryStream(tinResult?.TINData).CopyTo(stream);
      }

      return new CompactionExportResult(fullFileName);
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
      Log.LogInformation($"{nameof(GetTINSurface2)}: {Request.QueryString}");

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
        return new FileStreamResult(new MemoryStream(tinResult?.TINData), "application/ttm");

      if (Request.Headers["Accept"].Equals("application/ttm-and-metadata"))
        return Ok(new TTMAndMetaDatActioNResult
        {
          a = tinResult?.TINData.Length ?? 0,

          theFile = new FileStreamResult(new MemoryStream(tinResult?.TINData), "application/ttm")
        });

      return new FileStreamResult(new MemoryStream(tinResult?.TINData), "application/ttm")
      {
        FileDownloadName = "DecimatedTIN.ttm"
      };
    }

    private string BuildTINFilePath(string filename, string extension)
    {
      return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filename) + extension);
    }
  }
}
