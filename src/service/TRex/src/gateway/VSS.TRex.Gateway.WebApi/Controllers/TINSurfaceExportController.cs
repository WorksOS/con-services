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
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using FileSystem = System.IO.File;


namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// The controller for generating TIN surfaces decimated from elevation data
  /// </summary>
  public class TINSurfaceExportController : BaseController
  {
    /// <summary>
    /// Constructor for TIN surface export controller
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public TINSurfaceExportController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler,
      IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<TINSurfaceExportController>(), exceptionHandler, configStore)
    {
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

      Log.LogDebug($"Accept header is {Request.Headers[HeaderConstants.ACCEPT]}");

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


  private string BuildTINFilePath(string filename, string extension)
    {
      return Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filename) + extension);
    }
  }
}
