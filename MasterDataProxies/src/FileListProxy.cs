using System;
using System.Collections.Generic;
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
      {
      }
     
      public async Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null)
      {
        var result = await GetContainedMasterDataList<FileDataResult>(projectUid, "IMPORTED_FILE_CACHE_LIFE", "IMPORTED_FILE_API_URL", customHeaders,
          string.Format("?projectUid={0}", projectUid));
        if (result.Code == 0)
        {
          return result.ImportedFileDescriptors;
        }
        else
        {
          log.LogDebug("Failed to get list of files: {0}, {1}", result.Code, result.Message);
          return null;
        }
      }
  }
}
