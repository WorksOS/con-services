using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.Common.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.WebApiModels.FileAccess.ResultHandling;
using VSS.Raptor.Service.WebApiModels.FileAccess.Models;
using VSS.Raptor.Service.WebApiModels.FileAccess.Executors;
using VSS.Raptor.Service.Common.ResultHandling;
using System.Net;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Models;
using System;
using TCCFileAccess;


// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace VSS.Raptor.Service.WebApi.FileAccess.Controllers
{
  /// <summary>
  /// Controller for file access resources.
  /// </summary>
  public class FileAccessController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to get list of projects for customer
    /// </summary>
    private readonly IAuthenticatedProjectsStore authProjectsStore;

    /// <summary>
    /// Used to get a service provider
    /// </summary>
    private readonly IFileRepository fileAccess;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    /// <param name="fileAccess">TCC file repository</param>
    public FileAccessController(IASNodeClient raptorClient, ILoggerFactory logger,
      IAuthenticatedProjectsStore authProjectsStore, IFileRepository fileAccess)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<FileAccessController>();
      this.authProjectsStore = authProjectsStore;
      this.fileAccess = fileAccess;
    }

    /// <summary>
    /// Gets requested file for Raptor from TCC and stores in the specified location. 
    /// </summary>
    /// <param name="request">Details of the requested file</param>
    /// <returns>Returns JSON structure wtih operation result. {"Code":0,"Message":"User-friendly"}
    /// </returns>
    /// <executor>FileAccessExecutor</executor>
    [Route("api/v1/files")]
    [HttpPost]
    public FileAccessResult Post([FromBody]FileAccessRequest request)
    {
      log.LogInformation("Get file from TCC: " + Request.QueryString);
      try
      {
        request.Validate();
        return RequestExecutorContainer.Build<FileAccessExecutor>(logger, raptorClient, null, null, fileAccess).Process(request) as FileAccessResult;
      }
      finally
      {
        log.LogInformation("Get file from TCC returned: " + Response.StatusCode);
      }
    }

    /// <summary>
    /// Gets requested file for Raptor from TCC and returns it as a raw array of bytes. 
    /// </summary>
    /// <param name="request">Details of the requested file</param>
    /// <returns>File content as a bytes array.
    /// </returns>
    /// <executor>RawFileAccessExecutor</executor>
    [Route("api/v1/rawfiles")]
    public RawFileContainer PostRaw([FromBody]FileDescriptor request)
    {
      log.LogInformation("Get file from TCC as an array of bytes: " + Request.QueryString);
      try
      {
        request.Validate();
        RawFileAccessResult result = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, raptorClient, null, null, fileAccess).Process(request) as RawFileAccessResult;
        if (result != null)
        {
          return new RawFileContainer
          {
            fileContents = result.fileContents
          };
        }
        return null;
      }
      finally
      {
        log.LogInformation("Get file from TCC as an array of bytes: " + Response.StatusCode);
      }
    }
  }
}
