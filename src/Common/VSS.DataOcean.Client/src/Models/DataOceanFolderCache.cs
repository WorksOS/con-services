using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace VSS.DataOcean.Client.Models
{
  /// <summary>
  /// DO directory structure is identified by a series of DO internal 'folder ids',
  ///     starting with platform/customer/project, which contains a list of files.
  /// We want to only step this structure once and cache the folder ids, which are used as parents to the next level
  ///     so that subsequently we can go straight to that folder level
  /// </summary>
  public class DataOceanFolderCache
  {
    public IMemoryCache _cache;

    public DataOceanFolderCache(IMemoryCache memoryCache, string dataOceanRootFolderId)
    {
      _cache = memoryCache;
      var rootDataOceanFolderPath = new DataOceanFolderPath(dataOceanRootFolderId, new Dictionary<string, DataOceanFolderPath>());
      _cache.Set(dataOceanRootFolderId, rootDataOceanFolderPath);
    }
  }

  public class DataOceanFolderPath
  {
    public string DataOceanFolderId { get; }

    // subfolders and it FolderIds
    public IDictionary<string, DataOceanFolderPath> Nodes;

    public DataOceanFolderPath(string dataOceanFolderId, IDictionary<string, DataOceanFolderPath> nodes)
    {
      DataOceanFolderId = dataOceanFolderId;
      Nodes = nodes;
    }
  }
}
