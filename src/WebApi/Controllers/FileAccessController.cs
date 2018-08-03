using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.FileAccess.Service.Common.Models;
using VSS.Productivity3D.FileAccess.Service.WebAPI.Models.FileAccess.Executors;
using VSS.Productivity3D.FileAccess.Service.WebAPI.Models.FileAccess.ResultHandling;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Executors;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.Service.WebAPI.FileAccess.Controllers
{
  /// <summary>
  /// Controller for file access resources.
  /// </summary>
  public class FileAccessController : Controller
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to get a service provider
    /// </summary>
    private readonly IFileRepository fileAccess;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="fileAccess">TCC file repository</param>
    public FileAccessController(ILoggerFactory logger,
        IFileRepository fileAccess)
    {
      this.logger = logger;
      this.log = logger.CreateLogger<FileAccessController>();
      this.fileAccess = fileAccess;
    }

    /// <summary>
    /// Gets requested file for Raptor from TCC and returns it as an image/png. 
    /// </summary>
    /// <param name="request">Details of the requested file</param>
    /// <returns>File contents as an image/png.
    /// </returns>
    /// <executor>RawFileAccessExecutor</executor>
    [Route("api/v1/rawfiles")]
    [HttpPost]
    public FileResult PostRaw([FromBody] FileDescriptor request)
    {
      log.LogInformation("Get file from TCC as an image/png: " + JsonConvert.SerializeObject(request));
      try
      {
        request.Validate();
        var result = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, null, fileAccess).Process(request) as RawFileAccessResult;
        if (result != null)
        {
          return new FileStreamResult(new MemoryStream(result.fileContents), "image/png");
        }

        throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Raptor failed to return a file"));
      }
      finally
      {
        log.LogInformation("Get file from TCC as an image/png: " + Response.StatusCode);
      }
    }
  }
}
