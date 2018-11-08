using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Executors;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.WebAPI.Controllers
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
    public FileAccessController(ILoggerFactory logger, IFileRepository fileAccess)
    {
      this.logger = logger;
      log = logger.CreateLogger<FileAccessController>();
      this.fileAccess = fileAccess;
    }

    /// <summary>
    /// Gets requested file from TCC.
    /// </summary>
    [Route("api/v1/rawfiles")]
    [HttpPost]
    public FileResult PostRaw([FromBody] FileDescriptor request)
    {
      log.LogInformation($"Get file from TCC: {JsonConvert.SerializeObject(request)}");

      try
      {
        request.Validate();

        if (RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, null, fileAccess).Process(request) is RawFileAccessResult result)
        {
          return new FileStreamResult(new MemoryStream(result.fileContents), "application/octet-stream");
        }

        throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                $"Failed to return a file '{request.FilespaceId}/{request.FileName}'"));
      }
      finally
      {
        log.LogInformation($"Get file from TCC, response code: {Response.StatusCode}");
      }
    }
  }
}
