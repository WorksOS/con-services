using System;
using System.Collections.Generic;
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
    /// <summary>
    /// The cached set of sitemodels that are currently 'open' in TRex
    /// </summary>
    private Dictionary<Guid, ISiteModel> CachedModels = new Dictionary<Guid, ISiteModel>();

    //private IStorageProxy _ImmutableStorageProxy;
    private IStorageProxy _StorageProxy;
    private Func<IStorageProxy> StorageProxyFactory;

    /// <summary>
    /// The default storage proxy to be used for requests
    /// </summary>
    //public IStorageProxy ImmutableStorageProxy() => _ImmutableStorageProxy ?? (_ImmutableStorageProxy = DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage());
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

    public ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID, bool CreateIfNotExist)
    {
      ISiteModel result;

      lock (CachedModels)
      {
        if (CachedModels.TryGetValue(ID, out result))
          return result;
      }

      result = DIContext.Obtain<ISiteModel>();
      result.ID = ID;

      if (result.LoadFromPersistentStore(storageProxy) == FileSystemErrorStatus.OK)
      {
        lock (CachedModels)
        {
          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favour of the one in the dictionary
          if (CachedModels.TryGetValue(ID, out ISiteModel result2))
            return result2;

          CachedModels.Add(ID, result);
          return result;
        }
      }

      if (CreateIfNotExist)
      {
        lock (CachedModels)
        {
          // Check if another thread managed to get in before this thread. If so discard
          // the one just created in favour of the one in the dictionary
          if (CachedModels.TryGetValue(ID, out ISiteModel result2))
            return result2;

          CachedModels.Add(ID, result);
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
      // Remove or update if necessary the Sitemodel from any cached storage in this context
      GetSiteModel(StorageProxy, SiteModelID, false)?.LoadFromPersistentStore(StorageProxy);
    }
  }
}
