using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
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
    private readonly ITINSurfaceExportRequestor tINSurfaceExportRequestor;

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
    /// <param name="compactionSurfaceExportRequest"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/v1/export/surface/ttm")]
    public async Task<CompactionExportResult> PostTINSurface([FromBody] CompactionSurfaceExportRequest compactionSurfaceExportRequest)
    {
      Log.LogInformation($"{nameof(PostTINSurface)}: {Request.QueryString}");

      Log.LogDebug($"Accept header is {Request.Headers["Accept"]}");

      compactionSurfaceExportRequest.Validate();
      ValidateFilterMachines(nameof(PostTINSurface), compactionSurfaceExportRequest.ProjectUid, compactionSurfaceExportRequest.Filter);

      var tinResult = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(compactionSurfaceExportRequest)) as TINSurfaceExportResult;

      const string TTM_EXTENSION = ".ttm";
      const string ZIP_EXTENSION = ".zip";

      var fullFileName = BuildTINFilePath(compactionSurfaceExportRequest.FileName, ZIP_EXTENSION);

      if (FileSystem.Exists(fullFileName))
        FileSystem.Delete(fullFileName);

      using (var zipFile = ZipFile.Open(fullFileName, ZipArchiveMode.Create))
      {
        var entry = zipFile.CreateEntry(compactionSurfaceExportRequest.FileName + TTM_EXTENSION);
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
    public async Task<IActionResult> GetTINSurface2([FromQuery] Guid projectUid,
      [FromQuery] double? tolerance,
      [FromQuery] Guid? filterUid)
    {
      const string FILE_DOWNLOAD_NAME = "DecimatedTIN.ttm";

      Log.LogInformation($"{nameof(GetTINSurface2)}: {Request.QueryString}");

      Log.LogDebug($"Accept header is {Request.Headers["Accept"]}");

      var request = new TINSurfaceExportRequest
      {
        ProjectUid = projectUid,
        Tolerance = tolerance,
        Filter = FilterResult.CreateFilter(null) // Todo: Get the actual filter from the filterUid
      };

      request.Validate();
      ValidateFilterMachines(nameof(GetTINSurface2), request.ProjectUid, request.Filter);

      var container = RequestExecutorContainer.Build<TINSurfaceExportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler);

      var tinResult = await WithServiceExceptionTryExecuteAsync(() => container.ProcessAsync(request)) as TINSurfaceExportResult;

      if (Request.Headers["Accept"].Equals(ContentTypeConstants.ApplicationTTM))
        return new FileStreamResult(new MemoryStream(tinResult?.TINData), ContentTypeConstants.ApplicationTTM);

      if (Request.Headers["Accept"].Equals(ContentTypeConstants.ApplicationTTMAndMetaData))
        return Ok(new TTMAndMetaDatActioNResult
        {
          a = tinResult?.TINData.Length ?? 0,

          theFile = new FileStreamResult(new MemoryStream(tinResult?.TINData), ContentTypeConstants.ApplicationTTM)
        });

      return new FileStreamResult(new MemoryStream(tinResult?.TINData), ContentTypeConstants.ApplicationTTM)
      {
        FileDownloadName = FILE_DOWNLOAD_NAME
      };
    }

    private string BuildTINFilePath(string filename, string extension)
    {
      return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filename) + extension);
    }
  }
}
