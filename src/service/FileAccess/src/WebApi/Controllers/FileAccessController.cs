using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Http;
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
    private readonly ILogger log;
    private readonly ILoggerFactory logger;
    private readonly IFileRepository fileAccess;

    public FileAccessController(ILoggerFactory logger, IFileRepository fileAccess)
    {
      this.logger = logger;
      log = logger.CreateLogger<FileAccessController>();
      this.fileAccess = fileAccess;
    }

    /// <summary>
    /// Gets requested file from the injected repository.
    /// </summary>
    [HttpPost("api/v1/rawfiles")]
    public IActionResult GetFile([FromBody] FileDescriptor request)
    {
      log.LogInformation($"Get file from TCC: {JsonConvert.SerializeObject(request)}");

      try
      {
        request.Validate();

        if (RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, null, fileAccess)
                                    .Process(request) is RawFileAccessResult downloadResult && downloadResult.Success)
        {
          return File(downloadResult.fileContents, ContentTypeConstants.ApplicationOctetStream);
        }

        return NoContent();
      }
      finally
      {
        log.LogInformation($"Get file from TCC, response code: {Response.StatusCode}");
      }
    }
  }
}
