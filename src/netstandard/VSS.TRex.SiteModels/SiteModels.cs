using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// SiteModels contains a map of site model/data model identifiers (long) and SiteModel instances. 
  /// It may receive messages from the Ignite layer regarding invalidation of cache items...
  /// </summary>
  public class SiteModels : ISiteModels
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("SiteModels");

    /// <summary>
    /// The cached set of sitemodels that are currently 'open' in TRex
    /// </summary>
    private Dictionary<Guid, ISiteModel> CachedModels = new Dictionary<Guid, ISiteModel>();

    private IStorageProxy _StorageProxy;
    private Func<IStorageProxy> StorageProxyFactory;

    /// <summary>
    /// The default storage proxy to be used for requests
    /// </summary>
    public IStorageProxy StorageProxy
    {
      get => _StorageProxy ?? (_StorageProxy = StorageProxyFactory());
    }

    /// <summary>
    /// Default no-arg constructor. Made private to enforce prpvision of storage proxy
    /// </summary>
    private SiteModels() {}

    /// <summary>
    /// Constructs a SiteModels instance taking a storageProxyFactory delegate that will create the
    /// proorpoate primary storage proxy
    /// </summary>
    /// <param name="storageProxyFactory"></param>
    public SiteModels(Func<IStorageProxy> storageProxyFactory) : this()
    {
      StorageProxyFactory = storageProxyFactory;
    }

    public ISiteModel GetSiteModel(Guid ID) => GetSiteModel(StorageProxy, ID, false);

    public ISiteModel GetSiteModel(Guid ID, bool CreateIfNotExist) => GetSiteModel(StorageProxy, ID, CreateIfNotExist);

    public ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID) => GetSiteModel(storageProxy, ID, false);

    /// <summary>
    /// Retrieves a sitemodel from the persistent store ready for use. If the site model does not
    /// exist it will be created if CreateIfNotExist is true.
    /// </summary>
    /// <param name="storageProxy"></param>
    /// <param name="id"></param>
    /// <param name="createIfNotExist"></param>
    /// <returns></returns>
    public ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid id, bool createIfNotExist)
    {
      ISiteModel result;

      lock (CachedModels)
      {
        if (CachedModels.TryGetValue(id, out result))
          return result;
      }

      result = DIContext.Obtain<ISiteModelFactory>().NewSiteModel_NonTransient(id);

      Log.LogInformation($"Loading site model {id} from persistent store");

      if (result.LoadFromPersistentStore(storageProxy) == FileSystemErrorStatus.OK)
      {
        lock (CachedModels)
        {
          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favour of the one in the dictionary
          if (CachedModels.TryGetValue(id, out ISiteModel result2))
            return result2;

          CachedModels.Add(id, result);
          return result;
        }
      }

      Log.LogInformation($"Site model {id} is not present in the persistent store, createIfNotExist = {createIfNotExist}");

      if (createIfNotExist)
      {
        lock (CachedModels)
        {
          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favour of the one in the dictionary
          if (CachedModels.TryGetValue(id, out ISiteModel result2))
            return result2;

          Log.LogInformation($"Creating new site model {id} and adding to internal cache");

          CachedModels.Add(id, result);
          return result;
        }
      }

      return null;
    }

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the sitemodel to be reloaded
    /// </summary>
    /// <param name="SiteModelID"></param>
    public void SiteModelAttributesHaveChanged(Guid SiteModelID)
    {
      // Sitemodels have immutable characteristics in TRex. Multiple requests may reference the same site model
      // concurrently, with no interlocks enforcing access serialisation. Any attempt to replace or modify an already loaded
      // sitemodel may cause issue with concurrent request accessing that site model.
      // THe strategy here is to preserve continued access by concurrent requests to the sitemodel retrieved
      // at the time the request was initiated by removing it from the SiteModels cache but not destroying it.
      // Once all request based references to the sitemodel have completed the now orphaned sitemodel will be cleaned
      // up by the garbage collector. Removal of the sitemodel is interlocked with getting a sitemodel reference
      // to ensure no concurrency issues within the underlying cache implementation

      // Remove the site model from the cache
      lock (CachedModels)
      {
        if (CachedModels.ContainsKey(SiteModelID))
        {
          Log.LogInformation($"Evicting sitemodel {SiteModelID} from cache due to attribute change notification");

          CachedModels.Remove(SiteModelID);
        }
      }
    }
  }
}
