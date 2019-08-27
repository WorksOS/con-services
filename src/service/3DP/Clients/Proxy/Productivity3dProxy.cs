﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  /// <summary>
  /// Proxy for Raptor services.
  /// </summary>
  public class Productivity3dProxy : BaseProxy, IProductivity3dProxy
  {
    public Productivity3dProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }


    public async Task<BaseDataResult> InvalidateCache(string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(InvalidateCache)} Project UID: {projectUid}");
      var response = await SendRequest<BaseDataResult>("RAPTOR_NOTIFICATION_API_URL","" , customHeaders, "/invalidatecache", HttpMethod.Get, $"?projectUid={projectUid}");
      log.LogDebug($"{nameof(InvalidateCache)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

    public Task<Stream> GetLineworkFromAlignment(Guid projectUid, Guid alignmentUid, IDictionary<string, string> customHeaders)
    {
      var parameters = new Dictionary<string, string>
      {
        { "projectUid", projectUid.ToString() }, { "alignmentUid", alignmentUid.ToString() }
      };

      var queryParams = $"?{new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";
      var result = GetMasterDataStreamContent("RAPTOR_3DPM_API_URL", customHeaders, HttpMethod.Get, null, queryParams, "/linework/alignment");
      return result;
    }

    /// <summary>
    /// Validates the CoordinateSystem for the project.
    /// </summary>
    /// <param name="coordinateSystemFileContent">The content of the CS file.</param>
    /// <param name="coordinateSystemFileName">The filename.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CoordinateSystemValidate)} coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = new CoordinateSystemFileValidationRequest(coordinateSystemFileContent, coordinateSystemFileName);

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
      log.LogDebug($"{nameof(CoordinateSystemPost)} coordinateSystemFileName: {coordinateSystemFileName}");
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
      log.LogDebug($"{nameof(AddFile)} projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} dxfUnitsType: {dxfUnitsType}");
      var parameters = new Dictionary<string, string>
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
      log.LogDebug($"{nameof(DeleteFile)} projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} legacyFileId: {legacyFileId}");
      var parameters = new Dictionary<string, string>
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
      log.LogDebug($"{nameof(UpdateFiles)} projectUid: {projectUid} fileUids: {string.Join<Guid>(",", fileUids)}");
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
      log.LogDebug($"{nameof(NotifyImportedFileChange)} projectUid: {projectUid} fileUid: {fileUid}");
      var queryParams = $"?projectUid={projectUid}&fileUid={fileUid}";
      
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
      log.LogDebug($"{nameof(ValidateProjectSettings)} 1) projectUid: {projectUid}");
      var queryParams = $"?projectUid={projectUid}&projectSettings={projectSettings}";
      var response = await GetMasterDataItem<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", customHeaders, queryParams, "/validatesettings");
      log.LogDebug($"{nameof(ValidateProjectSettings)} 1) response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      
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
      log.LogDebug($"{nameof(ValidateProjectSettings)} 2) projectUid: {projectUid} settings type: {settingsType}");
      var queryParams = $"?projectUid={projectUid}&projectSettings={projectSettings}&settingsType={settingsType}";
      var response = await GetMasterDataItem<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", customHeaders, queryParams, "/validatesettings");
      log.LogDebug($"{nameof(ValidateProjectSettings)} 2) response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

      return response;
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    /// <param name="request">Description of the Project Settings request.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task<BaseDataResult> ValidateProjectSettings(ProjectSettingsRequest request, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(ValidateProjectSettings)} 3) projectUid: {request.projectUid}");
      var response = await SendRequest<BaseDataResult>("RAPTOR_PROJECT_SETTINGS_API_URL", JsonConvert.SerializeObject(request), customHeaders, "/validatesettings", HttpMethod.Post, string.Empty);
      log.LogDebug($"{nameof(ValidateProjectSettings)} 3) response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

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
      log.LogDebug($"{nameof(GetProjectStatistics)} projectUid: {projectUid}");
      var response = await SendRequest<ProjectStatisticsResult>("RAPTOR_PROJECT_SETTINGS_API_URL",
        string.Empty, customHeaders, "/projectstatistics", HttpMethod.Get, $"?projectUid={projectUid}");

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
      log.LogDebug($"{nameof(GetAlignmentPointsList)} projectUid: {projectUid}");
      var response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/alignmentpointslist", HttpMethod.Get, $"?projectUid={projectUid}");

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
      log.LogDebug($"{nameof(GetAlignmentPoints)}: projectUid: {projectUid}, alignmentUid: {alignmentUid}");
      var response = await SendRequest<AlignmentPointsResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/alignmentpoints", HttpMethod.Get, $"?projectUid={projectUid}&alignmentUid={alignmentUid}");

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
      log.LogDebug($"{nameof(GetDesignBoundaryPoints)} projectUid: {projectUid}, designUid: {designUid}");
      var response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/designboundarypoints", HttpMethod.Get, $"?projectUid={projectUid}&designUid={designUid}");

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
      log.LogDebug($"{nameof(GetFilterPoints)} projectUid: {projectUid}, filterUid: {filterUid}");
      var response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/filterpoints", HttpMethod.Get, $"?projectUid={projectUid}&filterUid={filterUid}");

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
      log.LogDebug($"{nameof(GetFilterPointsList)} projectUid={projectUid}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, boundaryType={boundaryType}");
      var response = await SendRequest<PointsListResult>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/filterpointslist", HttpMethod.Get, $"?projectUid={projectUid}&filterUid={filterUid}&baseUid={baseUid}&topUid={topUid}&boundaryType={boundaryType}");

      return response;
    }


    /// <summary>
    /// Gets a production data tile from the 3dpm WMS service.
    /// </summary>
    public async Task<byte[]> GetProductionDataTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, ushort width, ushort height, 
      string bbox, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null, bool explicitFilters = false)
    {
      log.LogDebug($"{nameof(GetProductionDataTile)}: projectUid={projectUid}, filterUid={filterUid}, width={width}, height={height}, mode={mode}, bbox={bbox}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}");

      var parameters = new Dictionary<string, string>
      {
        {"SERVICE", "WMS" }, {"VERSION" , "1.3.0" }, {"REQUEST", "GetMap" }, {"FORMAT", ContentTypeConstants.ImagePng}, {"TRANSPARENT", "true"},
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
      parameters.Add("explicitFilters", explicitFilters.ToString());
      var queryParams = $"?{new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";

      var request = new GracefulWebRequest(logger, configurationStore);
      var url = ExtractUrl("RAPTOR_3DPM_API_URL", "/productiondatatiles/png", queryParams);
      var stream = await request.ExecuteRequestAsStreamContent(url, method: HttpMethod.Get, customHeaders: customHeaders, retries: 3);
      return await stream.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Gets a "best fit" bounding box for the requested parameters.
    /// </summary>
    /// <returns>The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</returns>
    public async Task<string> GetBoundingBox(Guid projectUid, TileOverlayType[] overlays, Guid? filterUid, Guid? cutFillDesignUid, Guid? baseUid, 
      Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetBoundingBox)} projectUid={projectUid}, overlays={JsonConvert.SerializeObject(overlays)}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}");

      var parameters = new Dictionary<string, string>
      {
        {"projectUid", projectUid.ToString()}

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
        parameters.Add("baseUid", baseUid.ToString());
      }
      if (topUid.HasValue)
      {
        parameters.Add("topUid", topUid.ToString());
      }
      if (volCalcType.HasValue)
      {
        parameters.Add("volumeCalcType", volCalcType.ToString());
      }
      var queryParams = $"?{new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}&overlays={string.Join("&overlays=", overlays)}";

      string response = await SendRequest<string>("RAPTOR_3DPM_API_URL",
        string.Empty, customHeaders, "/raptor/boundingbox", HttpMethod.Get, queryParams);
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
      log.LogDebug($"{nameof(NotifyFilterChange)} filterUid: {filterUid}, projectUid: {projectUid}");
      var queryParams = $"?filterUid={filterUid}&projectUid={projectUid}";
      var response = await GetMasterDataItem<BaseDataResult>("RAPTOR_NOTIFICATION_API_URL", customHeaders, queryParams, "/filterchange");
      log.LogDebug($"{nameof(NotifyFilterChange)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

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
      log.LogDebug($"{nameof(NotifyFile)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

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
      var response = await SendRequest<CoordinateSystemSettingsResult>("COORDSYSPOST_API_URL", payload, customHeaders, route, HttpMethod.Post, string.Empty);
      log.LogDebug($"{nameof(CoordSystemPost)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

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
      log.LogDebug($"{nameof(UploadTagFile)} filename: {filename}, orgId: {orgId}");
      var request = CompactionTagFileRequest.CreateCompactionTagFileRequest(filename, data, orgId);
      var response = await SendRequest<BaseDataResult>("TAGFILEPOST_API_URL", JsonConvert.SerializeObject(request),
        customHeaders, "", HttpMethod.Post, string.Empty);
      log.LogDebug($"{nameof(UploadTagFile)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

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
      log.LogDebug($"{nameof(ExecuteGenericV1Request)} route: {route}");
      var response = await SendRequest<T>("RAPTOR_V1_BASE_API_URL", JsonConvert.SerializeObject(payload),
        customHeaders, route, HttpMethod.Post, string.Empty);
      log.LogDebug($"{nameof(ExecuteGenericV1Request)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
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
      log.LogDebug($"{nameof(ExecuteGenericV1Request)} route: {route}");
      var response = await SendRequest<T>("RAPTOR_V1_BASE_API_URL", string.Empty, customHeaders, route, HttpMethod.Get, query);
      log.LogDebug($"{nameof(ExecuteGenericV1Request)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

    /// <summary>
    /// Execute a generic request against v2 raptor endpoint
    /// </summary>
    /// <typeparam name="T">Expected response type</typeparam>
    /// <param name="route">Route on v1 endpoint</param>
    /// <param name="payload">Object to post</param>
    /// <param name="customHeaders">Authn\z headers</param>
    /// <returns></returns>
    public async Task<T> ExecuteGenericV2Request<T>(string route, HttpMethod method, Stream body = null, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(ExecuteGenericV2Request)} route: {route}");
      var response = await SendRequest<T>("RAPTOR_3DPM_API_URL", body, customHeaders, route, method);
      log.LogDebug($"{nameof(ExecuteGenericV2Request)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
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