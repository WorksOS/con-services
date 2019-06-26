﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class FileListProxy : BaseProxy, IFileListProxy
  {
      public FileListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(configurationStore, logger, cache)
      { }
     
      public async Task<List<FileData>> GetFiles(string projectUid, string userId, IDictionary<string, string> customHeaders)
      {
        var result = await GetContainedMasterDataList<FileDataResult>(projectUid, userId, 
          "IMPORTED_FILE_CACHE_LIFE", "IMPORTED_FILE_API_URL", customHeaders, $"?projectUid={projectUid}");
        if (result.Code == 0)
        {
          return result.ImportedFileDescriptors;
        }

        log.LogDebug($"Failed to get list of files: {result.Code} {result.Message}");
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
    /// Gets an imported file for a project. 
    /// </summary>
    public async Task<FileData> GetFileForProject(string projectUid, string userId, string importedFileUid,
      IDictionary<string, string> customHeaders = null)
    {
      return await GetItemWithRetry<FileDataResult, FileData>(GetFiles, f => string.Equals(f.ImportedFileUid, importedFileUid, StringComparison.OrdinalIgnoreCase), projectUid, userId, customHeaders);
    }
  }


}