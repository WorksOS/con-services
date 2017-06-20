using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
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
using VSS.Raptor.Service.WebApi.Compaction.Controllers;
using VSS.Raptor.Service.WebApiModels.Notification.Executors;
using VSS.Raptor.Service.WebApiModels.Notification.Models;
using WebApiModels.Notification.Models;
using MasterDataProxies;
using WebApiModels.Interfaces;

namespace VSS.Raptor.Service.WebApi.Notification
{
  [ResponseCache(NoStore = true)]
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
    /// For handling DXF tiles
    /// </summary>
    private ITileGenerator tileGenerator;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// Constructor with injected raptor client, logger and authenticated projects
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="authProjectsStore">Authenticated projects store</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="prefProxy">Proxy for user preferences</param>
    /// <param name="tileGenerator">DXF tile generator</param>
    /// <param name="fileListProxy">File list proxy</param>
    public NotificationController(IASNodeClient raptorClient, ILoggerFactory logger,
      IAuthenticatedProjectsStore authProjectsStore, IFileRepository fileRepo, IConfigurationStore configStore, 
      IPreferenceProxy prefProxy, ITileGenerator tileGenerator, IFileListProxy fileListProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionController>();
      this.authProjectsStore = authProjectsStore;
      this.fileRepo = fileRepo;
      this.configStore = configStore;
      this.prefProxy = prefProxy;
      this.tileGenerator = tileGenerator;
      this.fileListProxy = fileListProxy;
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <param name="fileId">A unique file identifier</param>
    /// <returns></returns>
    /// <executor>AddFileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetAddFile(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      log.LogDebug("GetAddFile: " + Request.QueryString);
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      long projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      var projectsById = authProjectsStore.GetProjectsById(customerUid);
      if (!projectsById.ContainsKey(projectId))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing Project or project does not belong to specified customer"));
      }
      string coordSystem = projectsById[projectId].coordinateSystemFileName;
      var customHeaders = Request.Headers.GetCustomHeaders();
      var userPrefs = await prefProxy.GetUserPreferences(customHeaders);
      var userUnits = userPrefs == null ? UnitsTypeEnum.US : (UnitsTypeEnum)Enum.Parse(typeof(UnitsTypeEnum), userPrefs.Units, true);
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, fileDes, coordSystem, userUnits, fileId);
      request.Validate();
      var executor = RequestExecutorContainer.Build<AddFileExecutor>(logger, raptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request);
      await ClearFilesCaches(projectUid, new List<Guid>(), customHeaders);//TODO: pass fileUid
      log.LogInformation("GetAddFile returned: " + Response.StatusCode);
      return result;
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>    /// <returns></returns>
    /// <param name="fileId">A unique file identifier</param>
    /// <executor>DeleteFileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/notification/deletefile")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetDeleteFile(
      [FromQuery] Guid projectUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      log.LogDebug("GetDeleteFile: " + Request.QueryString);
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      long projectId = ProjectID.GetProjectId(customerUid, projectUid, authProjectsStore);
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectId, projectUid, fileDes, null, UnitsTypeEnum.None, fileId);
      request.Validate();
      var executor = RequestExecutorContainer.Build<DeleteFileExecutor>(logger, raptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request);
      var customHeaders = Request.Headers.GetCustomHeaders();
      await ClearFilesCaches(projectUid, new List<Guid>(), customHeaders);//TODO: pass fileUid
      log.LogInformation("GetDeleteFile returned: " + Response.StatusCode);
      return result;
    }

    /// <summary>
    /// Notifies Raptor that files have been activated or deactivated
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUids">File UIDs</param>
    [ProjectUidVerifier]
    [Route("api/v2/notification/updatefiles")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetUpdateFiles(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid[] fileUids)
    {
      log.LogDebug("GetUpdateFiles: " + Request.QueryString);
      if (projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing projectUid parameter"));
      }
      if (fileUids.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing fileUids parameter"));
      }
      var customHeaders = Request.Headers.GetCustomHeaders();
      await ClearFilesCaches(projectUid, fileUids, customHeaders);
      log.LogInformation("GetUpdateFiles returned: " + Response.StatusCode);
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Update files notification successful");
    }

    /// <summary>
    /// Clears the imported files cache in the proxy and the response cache so that linework tile requests are refreshed appropriately
    /// </summary>
    /// <param name="projectUid">The project UID that the cached items belong to</param>
    /// <param name="fileUids">The file UIDs of files that have been activated/deactivated</param>
    /// <param name="customHeaders">The custom headers of the notification request</param>
    /// <returns></returns>
    private async  Task<bool> ClearFilesCaches(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders)
    {
      //Clear file list cache and reload
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
        customHeaders.Add("X-VisionLink-ClearCache", "true");

      var fileUidList = fileUids.Select(f => f.ToString());
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), customHeaders);
      var files = fileList.Where(f => fileUidList.Contains(f.ImportedFileUid)).ToList();
      var fileTypes = files.Select(f => f.ImportedFileType).Distinct();
 
      //Clear response cache for tiles for this project and file type(s)
      //TODO: Implement this
   
      return true;
    }

    /// <summary>
    /// Deserializes the file descriptor
    /// </summary>
    /// <param name="fileDescriptor">JSON representation of the file descriptor</param>
    /// <returns>The file descriptor instance</returns>
    private FileDescriptor GetFileDescriptor(string fileDescriptor)
    {
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
      return fileDes;
    }
  }
}
