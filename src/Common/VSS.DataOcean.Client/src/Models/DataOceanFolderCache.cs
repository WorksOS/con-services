using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace VSS.DataOcean.Client.Models
{
  // todo look at using DI singleton rather than transient in each of the services: Tile; Project

  /// <summary>
  /// DO directory structure is identified by a series of DO internal 'folder ids',
  ///     starting with DO rootFolder e.g.  rootFolder/customer/project, which contains a list of imported files.
  /// We want to only step this structure once and cache the folder ids, which are used as parents to the next level
  ///     so that subsequently we can go straight to that folder level
  /// </summary>
  public class DataOceanFolderCache
  {
    private readonly IMemoryCache _cache;

    public DataOceanFolderPath GetRootFolder(string rootFolder)
    {
      _cache.TryGetValue(rootFolder, out DataOceanFolderPath currentDataOceanFolderPath);
      return currentDataOceanFolderPath;
    }

    public DataOceanFolderCache(IMemoryCache memoryCache, string dataOceanRootFolderId)
    {
      _cache = memoryCache;
      var rootDataOceanFolderPath = new DataOceanFolderPath(dataOceanRootFolderId, new Dictionary<string, DataOceanFolderPath>());
      lock (_cache)
      {
        _cache.Set(dataOceanRootFolderId, rootDataOceanFolderPath);
      }
    }
  }

  public class DataOceanFolderPath
  {
    public string DataOceanFolderId { get; }

    // subfolders and it FolderIds
    public readonly IDictionary<string, DataOceanFolderPath> Nodes;

    public DataOceanFolderPath(string dataOceanFolderId, IDictionary<string, DataOceanFolderPath> nodes)
    {
      DataOceanFolderId = dataOceanFolderId;
      Nodes = nodes;
    }

    public DataOceanFolderPath CreateNode(string parentId, string folderName)
    {
      var folderPath = new DataOceanFolderPath(parentId, new Dictionary<string, DataOceanFolderPath>());

      lock (Nodes)
      {
        if (Nodes.TryGetValue(folderName, out var retrievedCurrentDataOceanFolderPath)) 
          return retrievedCurrentDataOceanFolderPath;
        Nodes.Add(folderName, folderPath);
        return folderPath;
      }
    }
  }
}
