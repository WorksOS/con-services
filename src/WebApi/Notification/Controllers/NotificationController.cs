using System;
using System.Net;
using System.Security.Principal;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCCFileAccess;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.Common.Utilities;
using VSS.Raptor.Service.WebApi.Compaction.Controllers;
using VSS.Raptor.Service.WebApiModels.Notification.Executors;
using VSS.Raptor.Service.WebApiModels.Notification.Models;


namespace VSS.Raptor.Service.WebApi.Notification
{
  public class NotificationController : Controller
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
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private IConfigurationStore configStore;
    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private IPreferenceProxy prefProxy;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="prefProxy">Proxy for user preferences</param>
    public NotificationController(IASNodeClient raptorClient, ILoggerFactory logger,
      IAuthenticatedProjectsStore authProjectsStore, IFileRepository fileRepo, IConfigurationStore configStore, IPreferenceProxy prefProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionController>();
      this.authProjectsStore = authProjectsStore;
      this.fileRepo = fileRepo;
      this.configStore = configStore;
      this.prefProxy = prefProxy;
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <returns></returns>
    /// <executor>AddFileExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public ContractExecutionResult GetAddFile(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] string fileDescriptor)
    {
      log.LogDebug("GetAddFile: " + Request.QueryString);
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      if (!projectId.HasValue)
      {
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      var projectsById = authProjectsStore.GetProjectsById(customerUid);
      if (!projectsById.ContainsKey(projectId.Value))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing Project or project does not belong to specified customer"));
      }
      string coordSystem = projectsById[projectId.Value].coordinateSystemFileName;
      var userPrefs = prefProxy.GetUserPreferences(Request.Headers.GetCustomHeaders()).Result;
      var userUnits = userPrefs == null ? "US" : userPrefs.Units;
      FileDescriptor fileDes = null;
      try
      {
        fileDes = JsonConvert.DeserializeObject<FileDescriptor>(fileDescriptor);
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            ex.Message));
      }

      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId.Value, projectUid, fileDes, coordSystem, userUnits);
      request.Validate();
      var result =
        RequestExecutorContainer.Build<AddFileExecutor>(logger, raptorClient, null, configStore, fileRepo).Process(request);
      log.LogInformation("GetAddFile returned: " + Response.StatusCode);
      return result;
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>    /// <returns></returns>
    /// <executor>DeleteFileExecutor</executor> 
    [ProjectIdVerifier]
    [ProjectUidVerifier]
    [Route("api/v2/notification/deletefile")]
    [HttpGet]
    public ContractExecutionResult GetDeleteFile(
      [FromQuery] long? projectId,
      [FromQuery] Guid? projectUid,
      [FromQuery] string fileDescriptor)
    {
      log.LogDebug("GetDeleteFile: " + Request.QueryString);
      if (!projectId.HasValue)
      {
        var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
        projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      }
      FileDescriptor fileDes = null;
      try
      {
        fileDes = JsonConvert.DeserializeObject<FileDescriptor>(fileDescriptor);
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            ex.Message));
      }
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId.Value, projectUid, fileDes, null, null);
      request.Validate();
      var result =
        RequestExecutorContainer.Build<DeleteFileExecutor>(logger, raptorClient, null, configStore, fileRepo).Process(request);
      log.LogInformation("GetDeleteFile returned: " + Response.StatusCode);
      return result;
    }
  }
}
