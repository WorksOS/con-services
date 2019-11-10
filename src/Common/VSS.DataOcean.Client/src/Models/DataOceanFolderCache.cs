using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.DataOcean.Client.Models
{
  /// <summary>
  /// DO directory structure is identified by a series of DO internal 'folder ids',
  ///     starting with DO rootFolder e.g.  rootFolder/customer/project, which contains a list of imported files.
  /// We want to only step this structure once and cache the folder ids, which are used as parents to the next level
  ///     so that subsequently we can go straight to that folder level
  /// 
  /// DataOceanClient must be instantiated in DI as a singleton,
  ///    so that memory of newly created Nodes will also be retained in memoryCache
  /// </summary>
  public class DataOceanFolderCache
  {
    private readonly IMemoryCache _cache;

    public DataOceanFolderPath GetRootFolder(string rootFolder)
    {
      DataOceanFolderPath currentDataOceanFolderPath;
      // lock is probably overkill as if DataOceanFolderCache hasn't instantiated yet, it can't get here
      lock (_cache)
      {
        var gotRootFolder = _cache.TryGetValue(rootFolder, out currentDataOceanFolderPath);
        if (!gotRootFolder || currentDataOceanFolderPath == null)
        {
          var message = $"{nameof(GetRootFolder)}: Failed to get dataOcean root folder {rootFolder} from cache.";
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
        }
      }

      return currentDataOceanFolderPath;
    }

    public DataOceanFolderCache(IMemoryCache memoryCache, string dataOceanRootFolderId)
    {
      _cache = memoryCache;
      var rootDataOceanFolderPath = new DataOceanFolderPath(dataOceanRootFolderId, new Dictionary<string, DataOceanFolderPath>());
      lock (_cache)
      {
        if (!_cache.TryGetValue<string>(dataOceanRootFolderId, out _))
        {
          _cache.Set(dataOceanRootFolderId, rootDataOceanFolderPath);
        }
      }
    }
  }
}
