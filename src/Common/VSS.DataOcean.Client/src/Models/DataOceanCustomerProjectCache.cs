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
  public class DataOceanCustomerProjectCache
  {
    private IMemoryCache _cache;

    public DataOceanCustomerProjectCache(IMemoryCache memoryCache)
    {
      _cache = memoryCache;
    }

    public CustomerFilePath GetCustomerFilePath(string customerUid)
    {
      _cache.TryGetValue(customerUid, out CustomerFilePath customerFilePath);
      return customerFilePath;
    }

    public CustomerFilePath GetOrCreateCustomerFilePath(string customerUid, string dataOceanCustomerFolderId)
    {
      if (!_cache.TryGetValue(customerUid, out CustomerFilePath cacheEntry))
      {
        cacheEntry = new CustomerFilePath(dataOceanCustomerFolderId);
        _cache.Set(customerUid, cacheEntry);
      }
      return cacheEntry;
    }

    public string GetProjectDataOceanFolderId(string customerUid, string projectUid)
    {
      if (!_cache.TryGetValue(customerUid, out CustomerFilePath cacheEntry))
        return null;

      cacheEntry.Projects.TryGetValue(projectUid, out var projectDataOceanFolderId);
      return projectDataOceanFolderId;
    }

    public string GetOrCreateProject(object customerKey, string projectUid, string projectDataOceanFolderId)
    {
      if (!_cache.TryGetValue(customerKey, out CustomerFilePath cacheEntry))
        return null;

      if (!cacheEntry.Projects.TryGetValue(projectUid, out var dataOceanId))
      {
        dataOceanId = projectDataOceanFolderId;
        cacheEntry.Projects.Add(new KeyValuePair<string, string>(projectUid, dataOceanId));
      }

      return dataOceanId;
    }

  }

  public class CustomerFilePath
  {
    public string DataOceanFolderId { get; }

    // VL projectUID and its DataOcean FolderId
    public IDictionary<string, string> Projects;

    public CustomerFilePath(string dataOceanFolderId)
    {
      DataOceanFolderId = dataOceanFolderId;
      Projects = new Dictionary<string, string>();
    }
  }
}
