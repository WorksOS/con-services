using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.Notification.Executors;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Notification.Controllers
{
  /// <summary>
  /// 
  /// </summary>
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
    /// For getting imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// For getting filter by Uid. Used here so FilterService can clear an item from cache.
    /// </summary>
    private readonly IFilterServiceProxy filterServiceProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="prefProxy">Proxy for user preferences</param>
    /// <param name="tileGenerator">DXF tile generator</param>
    /// <param name="fileListProxy">File list proxy</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public NotificationController(IASNodeClient raptorClient, ILoggerFactory logger,
      IFileRepository fileRepo, IConfigurationStore configStore,
      IPreferenceProxy prefProxy, ITileGenerator tileGenerator, IFileListProxy fileListProxy,
      IFilterServiceProxy filterServiceProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<NotificationController>();
      this.fileRepo = fileRepo;
      this.configStore = configStore;
      this.prefProxy = prefProxy;
      this.tileGenerator = tileGenerator;
      this.fileListProxy = fileListProxy;
      this.filterServiceProxy = filterServiceProxy;
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <param name="fileType">Type of the file</param>
    /// <param name="fileId">A unique file identifier</param>
    /// <returns>A code and message to indicate the result</returns>
    /// <executor>AddFileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetAddFile(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      log.LogDebug("GetAddFile: " + Request.QueryString);
      ProjectDescriptor projectDescr = (User as RaptorPrincipal).GetProject(projectUid);
      string coordSystem = projectDescr.coordinateSystemFileName;
      var customHeaders = Request.Headers.GetCustomHeaders();
      var userPrefs = await prefProxy.GetUserPreferences(customHeaders);
      var userUnits = userPrefs == null ? UnitsTypeEnum.US : (UnitsTypeEnum)Enum.Parse(typeof(UnitsTypeEnum), userPrefs.Units, true);
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectDescr.projectId, projectUid, fileDes, coordSystem, userUnits, fileId, fileType);
      request.Validate();
      var executor = RequestExecutorContainerFactory.Build<AddFileExecutor>(logger, raptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request);
      //Do we need to validate fileUid ?
      await ClearFilesCaches(projectUid, new List<Guid> { fileUid }, customHeaders);
      log.LogInformation("GetAddFile returned: " + Response.StatusCode);
      return result;
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param> 
    /// <param name="fileType">Type of the file</param>
    /// <param name="fileId">A unique file identifier</param>
    /// <returns>A code and message to indicate the result</returns>
    /// <executor>DeleteFileExecutor</executor> 
    [ProjectUidVerifier]
    [Route("api/v2/notification/deletefile")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetDeleteFile(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId)
    {
      log.LogDebug("GetDeleteFile: " + Request.QueryString);
      ProjectDescriptor projectDescr = (User as RaptorPrincipal).GetProject(projectUid);
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectDescr.projectId, projectUid, fileDes, null, UnitsTypeEnum.None, fileId, fileType);
      request.Validate();
      var executor = RequestExecutorContainerFactory.Build<DeleteFileExecutor>(logger, raptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request);
      await ClearFilesCaches(projectUid, new List<Guid> { fileUid }, Request.Headers.GetCustomHeaders());
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
    /// Notifies Raptor that a filterUid has been updated/deleted so clear it from the queue
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns>A code and message to indicate the result</returns>
    [Route("api/v2/notification/filterchange")]
    [HttpGet]
    public ContractExecutionResult GetNotifyFilterChange(
      [FromQuery] Guid filterUid)
    {
      log.LogDebug("GetNotifyFilterChange: " + Request.QueryString);
      filterServiceProxy.ClearCacheItem<string>(filterUid.ToString());
      log.LogInformation("GetNotifyFilterChange returned");
      return new ContractExecutionResult();
    }
    /// <summary>
    /// Clears the imported files cache in the proxy so that linework tile requests are refreshed appropriately
    /// </summary>
    /// <param name="projectUid">The project UID that the cached items belong to</param>
    /// <param name="fileUids">The file UIDs of files that have been activated/deactivated</param>
    /// <param name="customHeaders">The custom headers of the notification request</param>
    /// <returns>The updated list of imported files</returns>
    private async Task<List<FileData>> ClearFilesCaches(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders)
    {
      log.LogInformation("Clearing imported files cache for project {0}", projectUid);
      //Clear file list cache and reload
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
        customHeaders.Add("X-VisionLink-ClearCache", "true");

      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), customHeaders);
      log.LogInformation("After clearing cache {0} total imported files, {1} activated, for project {2}", fileList.Count, fileList.Count(f => f.IsActivated), projectUid);

      return fileList;
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
