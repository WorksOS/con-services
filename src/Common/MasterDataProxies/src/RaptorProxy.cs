using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
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


    public async Task<BaseDataResult> InvalidateCache(string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.InvalidateCache: Project UID: {projectUid}");
      BaseDataResult response = await SendRequest<BaseDataResult>("RAPTOR_NOTIFICATION_API_URL","" , customHeaders, "/invalidatecache", "GET", $"?projectUid={projectUid}");
      log.LogDebug("RaptorProxy.InvalidateCache: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }

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
    /// <param name="projectUid">Project UID</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetProjectStatistics: {projectUid}");
      ProjectStatisticsResult response = await SendRequest<ProjectStatisticsResult>("RAPTOR_PROJECT_SETTINGS_API_URL",
        string.Empty, customHeaders, "/projectstatistics", "GET", $"?projectUid={projectUid}");

      return response;
    }

    #region Tile Service Raptor requests
    /// <summary>
    /// Get the points for all active alignment files for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<PointsListResult> GetAlignmentPointsList(Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetAlignmentPointsList: {projectUid}");
      PointsListResult response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/alignmentpointslist", "GET", $"?projectUid={projectUid}");

      return response;
    }

    /// <summary>
    /// Get the points for an alignment file for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="alignmentUid">Alignment file UID</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<AlignmentPointsResult> GetAlignmentPoints(Guid projectUid, Guid alignmentUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetAlignmentPoints: projectUid={projectUid}, alignmentUid={alignmentUid}");
      AlignmentPointsResult response = await SendRequest<AlignmentPointsResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/alignmentpoints", "GET", $"?projectUid={projectUid}&alignmentUid={alignmentUid}");

      return response;
    }


    /// <summary>
    /// Get the boundary points for a design for a project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="designUid">Design UID</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<PointsListResult> GetDesignBoundaryPoints(Guid projectUid, Guid designUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetDesignBoundaryPoints: projectUid={projectUid}, designUid={designUid}");
      PointsListResult response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/designboundarypoints", "GET", $"?projectUid={projectUid}&designUid={designUid}");

      return response;
    }

    /// <summary>
    /// Get the boundary points for a filter for a project if it has a spatial filter
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<PointsListResult> GetFilterPoints(Guid projectUid, Guid filterUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetFilterPoints: projectUid={projectUid}, filterUid={filterUid}");
      PointsListResult response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/filterpoints", "GET", $"?projectUid={projectUid}&filterUid={filterUid}");

      return response;
    }

    /// <summary>
    /// Get the boundary points for the requested filters for a project if they have a spatial filter
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="baseUid">Volume base UID</param>
    /// <param name="topUid">Volume top UID</param>
    /// <param name="boundaryType">The type of spatial filter to get points for</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<PointsListResult> GetFilterPointsList(Guid projectUid, Guid? filterUid, Guid? baseUid, Guid? topUid, FilterBoundaryType boundaryType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetFilterPointsList: projectUid={projectUid}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, boundaryType={boundaryType}");
      PointsListResult response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/filterpointslist", "GET", $"?projectUid={projectUid}&filterUid={filterUid}&baseUid={baseUid}&topUid={topUid}&boundaryType={boundaryType}");

      return response;
    }


    /// <summary>
    /// Gets a production data tile from the 3dpm WMS service.
    /// </summary>
    public async Task<byte[]> GetProductionDataTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, ushort width, ushort height, 
      string bbox, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetBoundariesFromLinework: projectUid={projectUid}, filterUid={filterUid}, width={width}, height={height}, mode={mode}, bbox={bbox}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}");

      Dictionary<string, string> parameters = new Dictionary<string, string>
      {
        {"SERVICE", "WMS" }, {"VERSION" , "1.3.0" }, {"REQUEST", "GetMap" }, {"FORMAT", "image/png"}, {"TRANSPARENT", "true"},
        { "LAYERS", "Layers"}, {"CRS", "EPSG:4326" }, {"STYLES", string.Empty}, {"projectUid", projectUid.ToString()},
        { "mode", mode.ToString()}, {"width", width.ToString()}, {"height", height.ToString()}, {"bbox", bbox}
      };
      if (filterUid.HasValue)
      {
        parameters.Add("filterUid", filterUid.ToString());
      }
      if (cutFillDesignUid.HasValue)
      {
        parameters.Add("cutFillDesignUid", cutFillDesignUid.ToString());
      }
      if (baseUid.HasValue)
      {
        parameters.Add("volumeBaseUid", baseUid.ToString());
      }
      if (topUid.HasValue)
      {
        parameters.Add("volumeTopUid", topUid.ToString());
      }
      if (volCalcType.HasValue)
      {
        parameters.Add("volumeCalcType", volCalcType.ToString());
      }
      var queryParams = $"?{new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";

      var request = new GracefulWebRequest(logger, configurationStore);
      var url = ExtractUrl("RAPTOR_3DPM_API_URL", "/productiondatatiles/png", queryParams);
      var stream = await request.ExecuteRequestAsStreamContent(url, method: "GET", customHeaders: customHeaders, retries: 3);
      return await stream.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Gets a "best fit" bounding box for the requested parameters.
    /// </summary>
    /// <returns>The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</returns>
    public async Task<string> GetBoundingBox(Guid projectUid, TileOverlayType[] overlays, Guid? filterUid, Guid? cutFillDesignUid, Guid? baseUid, 
      Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.GetBoundingBox: projectUid={projectUid}, overlays={overlays}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}, headers {(customHeaders == null ? null : JsonConvert.SerializeObject(customHeaders))}");

      string filterParam = filterUid.HasValue ? $"&filterUid={filterUid}" : string.Empty;
      string cutFillDesignParam = cutFillDesignUid.HasValue ? $"&cutFillDesignUid={cutFillDesignUid}" : string.Empty;
      string baseParam = baseUid.HasValue ? $"&baseUid={baseUid}" : string.Empty;
      string topParam = topUid.HasValue ? $"&topUid={topUid}" : string.Empty;
      string volCalcTypeParam = volCalcType.HasValue ? $"&volumeCalcType={volCalcType}" : string.Empty;
      var overlaysParameter = string.Join("&overlays=", overlays);
      var queryParameters = $"?projectUid={projectUid}&overlays={overlaysParameter}{filterParam}{cutFillDesignParam}{baseParam}{topParam}{volCalcTypeParam}";

      string response = await SendRequest<string>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/boundingbox", "GET", queryParameters);
      return response;
    }
    #endregion

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

    /// <summary>
    /// Execute a generic request against v1 raptor endpoint
    /// </summary>
    /// <typeparam name="T">Expected response type</typeparam>
    /// <param name="route">Route on v1 endpoint</param>
    /// <param name="payload">Object to post</param>
    /// <param name="customHeaders">Authn\z headers</param>
    /// <returns></returns>
    public async Task<T> ExecuteGenericV1Request<T>(string route, object payload,
      IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.ExecuteGenericV1Request: route: {route}");
      var response = await SendRequest<T>("RAPTOR_V1_BASE_API_URL", JsonConvert.SerializeObject(payload),
        customHeaders, route, "POST", String.Empty);
      log.LogDebug("RaptorProxy.ExecuteGenericV1Request: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }

    /// <summary>
    /// Execute a generic request against v1 raptor endpoint
    /// </summary>
    /// <typeparam name="T">Expected response type</typeparam>
    /// <param name="route">Route on v1 endpoint</param>
    /// <param name="payload">Object to post</param>
    /// <param name="customHeaders">Authn\z headers</param>
    /// <returns></returns>
    public async Task<T> ExecuteGenericV1Request<T>(string route, string query, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"RaptorProxy.ExecuteGenericV1Request: route: {route}");
      var response = await SendRequest<T>("RAPTOR_V1_BASE_API_URL", string.Empty, customHeaders, route, "GET", query);
      log.LogDebug("RaptorProxy.ExecuteGenericV1Request: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
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