using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for Raptor services.
  /// </summary>
  public class RaptorProxy : BaseProxy, IRaptorProxy
  {
    public RaptorProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    /// <summary>
    /// Validates the CoordinateSystem for the project.
    /// </summary>
    /// <param name="coordinateSystemFileContent">The content of the CS file.</param>
    /// <param name="coordinateSystemFileName">The filename.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.CoordinateSystemValidate: coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = CoordinateSystemFileValidationRequest.CreateCoordinateSystemFileValidationRequest(coordinateSystemFileContent, coordinateSystemFileName);

      return await CoordSystemPost(JsonConvert.SerializeObject(payLoadToSend), customHeaders, "/validation");
    }

    /// <summary>
    /// Validates and posts to Raptor, the CoordinateSystem for the project.
    /// </summary>
    /// <param name="legacyProjectId">The legacy ProjectId.</param>
    /// <param name="coordinateSystemFileContent">The content of the CS file.</param>
    /// <param name="coordinateSystemFileName">The filename.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.CoordinateSystemPost: coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = CoordinateSystemFile.CreateCoordinateSystemFile(legacyProjectId, coordinateSystemFileContent, coordinateSystemFileName);

      return await CoordSystemPost(JsonConvert.SerializeObject(payLoadToSend), customHeaders, null);
    }

    /// <summary>
    /// Notifies Raptor that a file has been added to a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <param name="fileId">A unique file identifier (legacy)</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="fileType">Type of the file</param>
    /// <returns></returns>
    public async Task<AddFileResult> AddFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor, long fileId, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.AddFile: projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} dxfUnitsType: {dxfUnitsType}");

      Dictionary<string,string> parameters = new Dictionary<string, string>
      {
        { "projectUid", projectUid.ToString() }, {"fileType" , fileType.ToString() }, { "fileUid", fileUid.ToString() },
        { "fileDescriptor", fileDescriptor}, {"fileId", fileId.ToString()}, {"dxfUnitsType",dxfUnitsType.ToString()}
      };

      var queryParams = $"?{new System.Net.Http.FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";

      return await NotifyFile<AddFileResult>("/addfile", queryParams, customHeaders);
    }

    /// <summary>
    /// Notifies Raptor that a file has been deleted from a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="fileDescriptor">File descriptor in JSON format. Currently this is TCC filespaceId, path and filename</param>
    /// <param name="fileId">A unique file identifier (legcy)</param>
    /// <param name="fileType">Type of the file</param>
    /// <param name="legacyFileId"></param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    public async Task<BaseDataResult> DeleteFile(Guid projectUid, ImportedFileType fileType, Guid fileUid, string fileDescriptor, long fileId, long? legacyFileId, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.DeleteFile: projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} legacyFileId: {legacyFileId}");
      //var queryParams = $"?projectUid={projectUid}&fileType={fileType}&fileUid={fileUid}&fileDescriptor={fileDescriptor}&fileId={fileId}&legacyFileId={legacyFileId}";

      Dictionary<string, string> parameters = new Dictionary<string, string>
      {
        { "projectUid", projectUid.ToString() }, {"fileType" , fileType.ToString() }, { "fileUid", fileUid.ToString() },
        { "fileDescriptor", fileDescriptor}, {"fileId", fileId.ToString()}
      };

      var queryParams = $"?{new System.Net.Http.FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";

      return await NotifyFile<BaseDataResult>("/deletefile", queryParams, customHeaders);
    }

    /// <summary>
    ///  Notifies Raptor that files have been updated in a project
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUids">File UIDs</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    public async Task<BaseDataResult> UpdateFiles(Guid projectUid, IEnumerable<Guid> fileUids, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.UpdateFiles: projectUid: {projectUid} fileUids: {string.Join<Guid>(",", fileUids)}");
      var queryParams = $"?projectUid={projectUid}&fileUids={string.Join<Guid>("&fileUids=", fileUids)}";

      return await NotifyFile<BaseDataResult>("/updatefiles", queryParams, customHeaders);
    }

    /// <summary>
    /// Notifies Raptor that a file has been CRUD to a project via CGen
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="fileUid">File UID</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    public async Task<BaseDataResult> NotifyImportedFileChange(Guid projectUid, Guid fileUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.NotifyImportedFileChange: projectUid: {projectUid} fileUid: {fileUid}");
      var queryParams = $"?projectUid={projectUid}&fileUid={fileUid}";
      //log.LogDebug($"RaptorProxy.DeleteFile: queryParams: {JsonConvert.SerializeObject(queryParams)}");

      return await NotifyFile<BaseDataResult>("/importedfilechange", queryParams, customHeaders);
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="projectSettings">The projectSettings in Json to be validated.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<BaseDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.ProjectSettingsValidate: projectUid: {projectUid}");
      var queryParams = $"?projectUid={projectUid}&projectSettings={projectSettings}";
      BaseDataResult response = await GetMasterDataItem<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", customHeaders, queryParams, "/validatesettings");
      log.LogDebug("RaptorProxy.ProjectSettingsValidate: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="projectSettings">The projectSettings in Json to be validated.</param>
    /// <param name="settingsType">The projectSettings' type.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<BaseDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings, ProjectSettingsType settingsType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.ProjectSettingsValidate: projectUid: {projectUid}");
      var queryParams = $"?projectUid={projectUid}&projectSettings={projectSettings}&settingsType={settingsType}";
      BaseDataResult response = await GetMasterDataItem<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", customHeaders, queryParams, "/validatesettings");
      log.LogDebug("RaptorProxy.ProjectSettingsValidate: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    /// <param name="request">Description of the Project Settings request.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<BaseDataResult> ValidateProjectSettings(ProjectSettingsRequest request, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.ProjectSettingsValidate: projectUid: {request.projectUid}");
      BaseDataResult response = await SendRequest<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", JsonConvert.SerializeObject(request), customHeaders, "/validatesettings", "POST", String.Empty);
      log.LogDebug("RaptorProxy.ProjectSettingsValidate: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }


    /// <summary>
    /// Get the statistics for a project.
    /// </summary>
    /// <param name="request">Description of the Project Settings request.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetProjectStatistics: {projectUid}");
      ProjectStatisticsResult response = await SendRequest<ProjectStatisticsResult>("RAPTOR_PROJECT_SETTINGS_API_URL",
        string.Empty, customHeaders, "/projectstatistics", "GET", $"projectUid={projectUid}");

      return response;
    }



    /// <summary>
    /// Validates that filterUid has changed i.e. updated/deleted but not inserted
    /// </summary>
    /// <param name="filterUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<BaseDataResult> NotifyFilterChange(Guid filterUid, Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.NotifyFilterChange: filterUid: {filterUid}, projectUid: {projectUid}");
      var queryParams = $"?filterUid={filterUid}&projectUid={projectUid}";
      BaseDataResult response = await GetMasterDataItem<BaseDataResult>("RAPTOR_NOTIFICATION_API_URL", customHeaders, queryParams, "/filterchange");
      log.LogDebug("RaptorProxy.NotifyFilterChange: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    /// <summary>
    ///  Notifies Raptor that a file has been added to or deleted from a project
    /// </summary>
    /// <param name="route">The route for add or delete file notification</param>
    /// <param name="queryParams">Query parameters for the request</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    private async Task<T> NotifyFile<T>(string route, string queryParams, IDictionary<string, string> customHeaders)
    {
      T response = await GetMasterDataItem<T>("RAPTOR_NOTIFICATION_API_URL", customHeaders, queryParams, route);
      var message = string.Format("RaptorProxy.NotifyFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      log.LogDebug(message);

      return response;
    }

    /// <summary>
    /// Posts the coordinate system to Raptor
    /// </summary>
    /// <param name="payload">The payload to send (request body)</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <param name="route">Additional routing to add to the base URL</param>
    /// <returns></returns>
    private async Task<CoordinateSystemSettingsResult> CoordSystemPost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      CoordinateSystemSettingsResult response = await SendRequest<CoordinateSystemSettingsResult>("COORDSYSPOST_API_URL", payload, customHeaders, route, "POST", String.Empty);
      log.LogDebug("RaptorProxy.CoordSystemPost: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }


    /// <summary>
    /// Uploads the tag file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="data">The data.</param>
    /// <param name="orgId">The org identifier.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    public async Task<BaseDataResult> UploadTagFile(string filename, byte[] data, string orgId = null, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.UploadTagFile: filename: {filename}, orgId: {orgId}");
      var request = CompactionTagFileRequest.CreateCompactionTagFileRequest(filename, data, orgId);
      var response = await SendRequest<BaseDataResult>("TAGFILEPOST_API_URL", JsonConvert.SerializeObject(request),
        customHeaders, "", "POST", String.Empty);
      log.LogDebug("RaptorProxy.UploadTagFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }

  }

  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  internal class CompactionTagFileRequest
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid? projectUid { get; private set; }

    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    /// <value>Required. Shall contain only ASCII characters. Maximum length is 256 characters.</value>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    public string fileName { get; private set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(PropertyName = "data", Required = Required.Always)]
    public byte[] data { get; private set; }


    /// <summary>
    /// Defines Org ID (either from TCC or Connect) to support project-based subs
    /// </summary>
    [JsonProperty(PropertyName = "OrgID", Required = Required.Default)]
    public string OrgID { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionTagFileRequest()
    {
    }

    /// <summary>
    /// Create instance of CompactionTagFileRequest
    /// </summary>
    /// <param name="fileName">file name</param>
    /// <param name="data">metadata</param>
    /// <param name="projectUid">project UID</param>
    /// <returns></returns>
    public static CompactionTagFileRequest CreateCompactionTagFileRequest(
      string fileName,
      byte[] data,
      string orgId = null,
      Guid? projectUid = null)
    {
      return new CompactionTagFileRequest
      {
        fileName = fileName,
        data = data,
        OrgID = orgId,
        projectUid = projectUid
      };
    }
  }
}