using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Notification.Executors;
using VSS.Productivity3D.WebApi.Models.Services;
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Notification.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class NotificationController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;
    /// <summary>
    /// For handling DXF tiles
    /// </summary>
    private readonly ITileGenerator tileGenerator;

    /// <summary>
    /// For getting imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// For getting filter by Uid. Used here so FilterService can clear an item from cache.
    /// </summary>
    private readonly IFilterServiceProxy filterServiceProxy;

    private readonly IResponseCache cache;
    /// <summary>
    /// User preferences interface
    /// </summary>
    private readonly IPreferenceProxy userPreferences;

    /// <summary>
    /// Project list proxy
    /// </summary>
    private readonly IProjectListProxy projectsListProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    public NotificationController(IASNodeClient raptorClient, ILoggerFactory logger,
      IFileRepository fileRepo, IConfigurationStore configStore,
      IPreferenceProxy prefProxy, ITileGenerator tileGenerator, IFileListProxy fileListProxy,
      IFilterServiceProxy filterServiceProxy, IResponseCache cache, IProjectListProxy projectProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<NotificationController>();
      this.fileRepo = fileRepo;
      this.configStore = configStore;
      this.tileGenerator = tileGenerator;
      this.fileListProxy = fileListProxy;
      this.filterServiceProxy = filterServiceProxy;
      this.cache = cache;
      userPreferences = prefProxy;
      projectsListProxy = projectProxy;
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <param name="fileType">Type of the file</param>
    /// <param name="fileId">A unique file identifier</param>
    /// <param name="dxfUnitsType">A DXF file units type</param>
    /// <param name="fileQueue"></param>
    [ProjectUidVerifier]
    [Route("api/v2/notification/addfile")]
    [HttpGet]
    public async Task<Models.Notification.Models.AddFileResult> GetAddFile(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromServices] IEnqueueItem<ProjectFileDescriptor> fileQueue)
    {
      log.LogDebug("GetAddFile: " + Request.QueryString);
      ProjectData projectDescr = await ((RaptorPrincipal) User).GetProject(projectUid);
      string coordSystem = projectDescr.CoordinateSystemFileName;
      var customHeaders = Request.Headers.GetCustomHeaders();
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);

      if (fileType == ImportedFileType.Alignment)
      {
        var preferences = await userPreferences.GetUserPreferences(Request.Headers.GetCustomHeaders());
        if (preferences == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to retrieve preferences for current user"));
        }

        var units = preferences.Units.UnitsType();

        switch (units)
        {
          case UnitsTypeEnum.Metric:
            dxfUnitsType = DxfUnitsType.Meters;
            break;
          case UnitsTypeEnum.Imperial:
            dxfUnitsType = DxfUnitsType.ImperialFeet;
            break;
          case UnitsTypeEnum.US:
            dxfUnitsType = DxfUnitsType.UsSurveyFeet;
            break;
        }
      }

      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(
        projectDescr.LegacyProjectId,
        projectUid, fileDes,
        coordSystem,
        dxfUnitsType,
        fileId,
        fileType);

      request.Validate();
      /*var executor = RequestExecutorContainerFactory.Build<AddFileExecutor>(logger, RaptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request) as Models.Notification.Models.AddFileResult;*/
      //Instead, leverage the service
      fileQueue.EnqueueItem(request);
      //Do we need to validate fileUid ?
      await ClearFilesCaches(projectUid, customHeaders);
      cache.InvalidateReponseCacheForProject(projectUid);
      log.LogInformation("GetAddFile returned: " + Response.StatusCode);
      return new Models.Notification.Models.AddFileResult(ContractExecutionStatesEnum.ExecutedSuccessfully,
        "Add file notification successful");
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary> 
    [ProjectUidVerifier]
    [Route("api/v2/notification/deletefile")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetDeleteFile(
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType fileType,
      [FromQuery] Guid fileUid,
      [FromQuery] string fileDescriptor,
      [FromQuery] long fileId,
      [FromQuery] long? legacyFileId
      )
    {
      log.LogDebug("GetDeleteFile: " + Request.QueryString);
      ProjectData projectDescr = await ((RaptorPrincipal) User).GetProject(projectUid);
      var customHeaders = Request.Headers.GetCustomHeaders();

      //Cannot delete a design or alignment file that is used in a filter
      //TODO: When scheduled reports are implemented, extend this check to them as well.
      if (fileType == ImportedFileType.DesignSurface || fileType == ImportedFileType.Alignment)
      {
        var filters = await GetFilters(projectUid, Request.Headers.GetCustomHeaders(true));
        if (filters != null)
        {
          var fileUidStr = fileUid.ToString();
          if (filters.Any(f => f.DesignUid == fileUidStr || f.AlignmentUid == fileUidStr))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Cannot delete a design surface or alignment file used in a filter"));
          }
        }
      }
      FileDescriptor fileDes = GetFileDescriptor(fileDescriptor);
      var request = ProjectFileDescriptor.CreateProjectFileDescriptor(projectDescr.LegacyProjectId, projectUid, fileDes, null, DxfUnitsType.Meters, fileId, fileType, legacyFileId);
      request.Validate();
      var executor = RequestExecutorContainerFactory.Build<DeleteFileExecutor>(logger, raptorClient, null, configStore, fileRepo, tileGenerator);
      var result = await executor.ProcessAsync(request);
      await ClearFilesCaches(projectUid, customHeaders);
      cache.InvalidateReponseCacheForProject(projectUid);
      log.LogInformation("GetDeleteFile returned: " + Response.StatusCode);
      return result;
    }


    /// <summary>
    /// Notifies Raptor that files have been activated or deactivated
    /// </summary>
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
      await ClearFilesCaches(projectUid, customHeaders);
      cache.InvalidateReponseCacheForProject(projectUid);
      log.LogInformation("GetUpdateFiles returned: " + Response.StatusCode);
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Update files notification successful");
    }

    /// <summary>
    /// Dumps cache in the ResponseCache and Masterdata cache for a project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    [ProjectUidVerifier]
    [Route("api/v2/notification/invalidatecache")]
    [HttpGet]
    public async Task<ContractExecutionResult> InvalidateCache([FromQuery] Guid projectUid)
    {
      var customHeaders = Request.Headers.GetCustomHeaders();
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
        customHeaders.Add("X-VisionLink-ClearCache", "true");
      await projectsListProxy.GetProjectForCustomer(((RaptorPrincipal) User).CustomerUid, projectUid.ToString(),
        customHeaders);
      cache.InvalidateReponseCacheForProject(projectUid);
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Notifies Raptor that a file has been CRUD to a project via CGen
    ///      This is called by the SurveyedSurface sync during Lift and Shift/Beta period.
    ///      When a file is added via CGen flexGateway, it will tell raptor.
    ///        However the 3dp UI needs to know about the change, so needs to refresh its caches.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v2/notification/importedfilechange")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetNotifyImportedFileChange(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid fileUid)
    {
      log.LogDebug("GetNotifyImportedFileChange: " + Request.QueryString);
      var customHeaders = Request.Headers.GetCustomHeaders();
      await ClearFilesCaches(projectUid, customHeaders);
      cache.InvalidateReponseCacheForProject(projectUid);
      log.LogInformation("GetNotifyImportedFileChange returned");
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Notifies Raptor that a filterUid has been updated/deleted so clear it from the queue
    /// </summary>
    [Route("api/v2/notification/filterchange")]
    [HttpGet]
    public ContractExecutionResult GetNotifyFilterChange(
      [FromQuery] Guid filterUid, [FromQuery] Guid projectUid)
    {
      log.LogDebug("GetNotifyFilterChange: " + Request.QueryString);
      filterServiceProxy.ClearCacheItem(filterUid.ToString());
      filterServiceProxy.ClearCacheListItem(projectUid.ToString());
      log.LogInformation("GetNotifyFilterChange returned");
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Clears the imported files cache in the proxy so that linework tile requests are refreshed appropriately
    /// </summary>
    private async Task<List<FileData>> ClearFilesCaches(Guid projectUid, IDictionary<string, string> customHeaders)
    {
      log.LogInformation("Clearing imported files cache for project {0}", projectUid);
      //Clear file list cache and reload
      if (!customHeaders.ContainsKey("X-VisionLink-ClearCache"))
        customHeaders.Add("X-VisionLink-ClearCache", "true");

      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), GetUserId(), customHeaders);
      log.LogInformation("After clearing cache {0} total imported files, {1} activated, for project {2}", fileList.Count, fileList.Count(f => f.IsActivated), projectUid);

      return fileList;
    }

    /// <summary>
    /// Deserializes the file descriptor
    /// </summary>
    private FileDescriptor GetFileDescriptor(string fileDescriptor)
    {
      FileDescriptor fileDes;
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

    /// <summary>
    /// Get the list of filters for the project
    /// </summary>
    private async Task<List<Filter>> GetFilters(Guid projectUid, IDictionary<string, string> customHeaders)
    {
      var filterDescriptors = await filterServiceProxy.GetFilters(projectUid.ToString(), customHeaders);
      if (filterDescriptors == null || filterDescriptors.Count == 0)
      {
        return null;
      }

      return filterDescriptors.Select(f => JsonConvert.DeserializeObject<Filter>(f.FilterJson)).ToList();
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}
