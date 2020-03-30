using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  public class Productivity3dV2ProxyNotification : Productivity3dV2Proxy, IProductivity3dV2ProxyNotification
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Productivity3D;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PRODUCTIVITY3D_NOTIFICATION_CACHE_LIFE"; // not used

    public Productivity3dV2ProxyNotification(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Notifies TRex/Raptor that a file has been added to a project
    /// </summary>
    public async Task<AddFileResult> AddFile(string projectUid, ImportedFileType fileType, string fileUid, string fileDescriptor, long fileId, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(AddFile)} projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} dxfUnitsType: {dxfUnitsType}");
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid), new KeyValuePair<string, string>("fileType", fileType.ToString()),
        new KeyValuePair<string, string>("fileUid", fileUid), new KeyValuePair<string, string>("fileDescriptor", fileDescriptor),
        new KeyValuePair<string, string>("fileId", fileId.ToString()), new KeyValuePair<string, string>("dxfUnitsType", dxfUnitsType.ToString())
      };

      return await NotifyFile<AddFileResult>("/notification/addfile", queryParams, customHeaders);
    }

    /// <summary>
    /// Notifies TRex/Raptor that a file has been deleted from a project
    /// </summary>
    public async Task<BaseMasterDataResult> DeleteFile(string projectUid, ImportedFileType fileType, string fileUid, string fileDescriptor, long fileId, long? legacyFileId, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteFile)} projectUid: {projectUid} fileUid: {fileUid} fileDescriptor: {fileDescriptor} fileId: {fileId} legacyFileId: {legacyFileId}");
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid), new KeyValuePair<string, string>("fileType", fileType.ToString()),
        new KeyValuePair<string, string>("fileUid", fileUid), new KeyValuePair<string, string>("fileDescriptor", fileDescriptor),
        new KeyValuePair<string, string>("fileId", fileId.ToString())
      };

      return await NotifyFile<BaseMasterDataResult>("/notification/deletefile", queryParams, customHeaders);
    }

    /// <summary>
    ///  Notifies TRex/Raptor that files have been updated in a project
    /// </summary>
    /// <returns></returns>
    public async Task<BaseMasterDataResult> UpdateFiles(string projectUid, IEnumerable<string> fileUids, IDictionary<string, string> customHeaders = null)
    {
      var fileUidsList = fileUids.ToList();
      log.LogDebug($"{nameof(UpdateFiles)} projectUid: {projectUid} fileUids: {string.Join<string>(",", fileUidsList)}");

      //var queryParams = $"?projectUid={projectUid}&fileUids={string.Join<Guid>("&fileUids=", fileUids)}";
      var queryParams = new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>("projectUid", projectUid)};
      foreach (var fileUid in fileUidsList)
        queryParams.Add(new KeyValuePair<string, string>("fileUids", fileUid));

      return await NotifyFile<BaseMasterDataResult>("/notification/updatefiles", queryParams, customHeaders);
    }

    /// <summary>
    /// Notifies TRex/Raptor that a file has been CRUD to a project via CGen
    /// </summary>
    public async Task<BaseMasterDataResult> NotifyImportedFileChange(string projectUid, string fileUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(NotifyImportedFileChange)} projectUid: {projectUid} fileUid: {fileUid}");
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid),
        new KeyValuePair<string, string>("fileUid", fileUid)
      };

      return await NotifyFile<BaseMasterDataResult>("/notification/importedfilechange", queryParams, customHeaders);
    }

    public async Task<BaseMasterDataResult> InvalidateCache(string projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(InvalidateCache)} Project UID: {projectUid}");
      var queryParams = new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>("projectUid", projectUid)};

      var response = await GetMasterDataItemServiceDiscoveryNoCache<BaseMasterDataResult>("/invalidatecache", customHeaders, queryParams);
      log.LogDebug($"{nameof(InvalidateCache)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

    /// <summary>
    /// Validates that filterUid has changed i.e. updated/deleted but not inserted
    /// </summary>
    public async Task<BaseMasterDataResult> NotifyFilterChange(string filterUid, string projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(NotifyFilterChange)} filterUid: {filterUid}, projectUid: {projectUid}");
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("filterUid", filterUid),
        new KeyValuePair<string, string>("projectUid", projectUid)
      };

      var response = await GetMasterDataItemServiceDiscoveryNoCache<BaseMasterDataResult>("/notification/filterchange", customHeaders, queryParams );
      log.LogDebug($"{nameof(NotifyFilterChange)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

      return response;
    }

    /// <summary>
    ///  Notifies TRex/Raptor that a file has been added to or deleted from a project
    /// </summary>
    private async Task<T> NotifyFile<T>(string route, IList<KeyValuePair<string, string>> queryParams, IDictionary<string, string> customHeaders)
      where T : class, IMasterDataModel
    {
      T response = await GetMasterDataItemServiceDiscoveryNoCache<T>(route, customHeaders, queryParams);
      log.LogDebug($"{nameof(NotifyFile)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

      return response;
    }
  }
}
