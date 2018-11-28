using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class FileListProxy : BaseProxy, IFileListProxy
  {
      public FileListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
      { }
     
      public async Task<List<FileData>> GetFiles(string projectUid, string userId, IDictionary<string, string> customHeaders)
      {
        var result = await GetContainedMasterDataList<FileDataResult>(projectUid, userId, 
          "IMPORTED_FILE_CACHE_LIFE", "IMPORTED_FILE_API_URL", customHeaders, $"?projectUid={projectUid}");
        if (result.Code == 0)
        {
          return result.ImportedFileDescriptors;
        }

        log.LogDebug("Failed to get list of files: {0}, {1}", result.Code, result.Message);
        return null;
      }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="projectUid">The projectUid of the item to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string projectUid, string userId)
    {
      ClearCacheItem<FileDataResult>(projectUid, userId);
    }

    /// <summary>
    /// Get a file from the list of files for a project.
    /// </summary>
    private async Task<FileData> GetFileFromList(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null)
    {
      var list = await GetFiles(projectUid, userId, customHeaders);
      return list.SingleOrDefault(f => string.Equals(f.ImportedFileUid, importedFileUid, StringComparison.OrdinalIgnoreCase));

    }

    /// <summary>
    /// Gets an imported file for a project. The GetFiles method has an additional parameter to other 'get list' methods
    /// therefore we cannot reuse the BaseProxy.GetItemWithRetry method.
    /// </summary>
    public async Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null)
    {
      var file = await GetFileFromList(projectUid, userId, importedFileUid, customHeaders);
      if (file == null)
      {
        ClearCacheItem(projectUid, userId);
        file = await GetFileFromList(projectUid, userId, importedFileUid, customHeaders);
      }
      return file;
    }
  }


}
