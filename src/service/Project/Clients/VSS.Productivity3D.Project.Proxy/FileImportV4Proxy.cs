using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.Productivity3D.Project.Proxy
{
  public class FileImportV4Proxy : BaseServiceDiscoveryProxy, IFileImportProxy
  {
    public FileImportV4Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Project;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V4;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "IMPORTED_FILE_CACHE_LIFE";

    public async Task<List<FileData>> GetFiles(string projectUid, string userId, IDictionary<string, string> customHeaders)
      {
        var result = await GetMasterDataItemServiceDiscovery<FileDataResult>
        ("/importedfiles", projectUid, userId, customHeaders,
          new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectUid", projectUid) });

        if (result.Code == 0)
          return result.ImportedFileDescriptors;

        log.LogDebug("Failed to get list of files: {0}, {1}", result.Code, result.Message);
        return null;
      }

    /// <summary>
    /// Gets an imported file for a project. 
    /// </summary>
    public async Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<FileDataResult, FileData>(GetFiles, f => string.Equals(f.ImportedFileUid, importedFileUid, StringComparison.OrdinalIgnoreCase), projectUid, userId, customHeaders);
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="projectUid">The projectUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    [Obsolete("Use the Base ClearCacheByTag method")]
    public void ClearCacheItem(string projectUid, string userId = null)
    {
      ClearCacheByTag(projectUid);
      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }

  }
}
