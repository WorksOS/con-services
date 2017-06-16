using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using MasterDataProxies.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;

namespace MasterDataProxies
{
  public class FileListProxy : BaseProxy, IFileListProxy
  {
    private static TimeSpan fileListCacheLife = new TimeSpan(0, 15, 0);//TODO: how long to cache ?

    public FileListProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
      
    }

    public async Task<List<FileData>> GetFiles(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetContainedList<FileDataResult>(projectUid, fileListCacheLife, "IMPORTED_FILE_API_URL", customHeaders,
        string.Format("?projectUid={0}", projectUid));
      if (result.Code == 0)
      {
        return result.FileDescriptors;
      }
      else
      {
        log.LogDebug("Failed to get list of files: {0}, {1}", result.Code, result.Message);
        return null;
      }
    }
  }
}
