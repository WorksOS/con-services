using System;
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
    //  Dictionary<Guid, SiteModel> CachedModels = new Dictionary<Guid, SiteModel>()

    //private IStorageProxy _ImmutableStorageProxy;
    private IStorageProxy _StorageProxy;

    /// <summary>
    /// The default storage proxy to be used for requests
    /// </summary>
    //public IStorageProxy ImmutableStorageProxy() => _ImmutableStorageProxy ?? (_ImmutableStorageProxy = DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage());
    public IStorageProxy StorageProxy
    {
      get => _StorageProxy;
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SiteModels()
    {
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SiteModels(IStorageProxy storageProxy) : this()
    {
      _StorageProxy = storageProxy;
    }

    public ISiteModel GetSiteModel(Guid ID) => GetSiteModel(StorageProxy, ID, false);

    public ISiteModel GetSiteModel(Guid ID, bool CreateIfNotExist) => GetSiteModel(StorageProxy, ID, CreateIfNotExist);

    public ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID) => GetSiteModel(storageProxy, ID, false);

    public ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID, bool CreateIfNotExist)
    {
      SiteModel result = new SiteModel(ID);

      if (result.LoadFromPersistentStore(storageProxy) == FileSystemErrorStatus.OK)
      {
        return result;
      }
      else
      {
        // The SiteModel does not exist - create a new one if requested
        return CreateIfNotExist ? result : null;
      }

      /*
       // The commented out code in this block operates by maintaining a dictionary if sitemodels. In Raptor
          this was supported by significant locking mechanisms. In TRex, the code above simple creates a new sitemodel
          each time on demand, however, it may be useful for performance reasons to revert to the dictionary approach
          but clear the element in the dictionary whenever the processing layer advises the sitemodel has changed.
          In order to support performant 'create once per access', the sitemodel itself shoudl ahve minimal serialisaed
          content delagating non trivial blocks of information to additional cache elements that are loaded on demand
          in the context of the operating request.

          lock (this)
          {
              if (!CachedModels.TryGetValue(ID, out result))
              {
                  result = new SiteModel(ID);

                  if (result.LoadFromPersistentStore(storageProxy) == FileSystemErrorStatus.OK)
                  {
                      CachedModels.Add(ID, result);
                  }
                  else
                  {
                      // The SiteModel does not exist in the store - create a new one if requested
                      if (CreateIfNotExist)
                      {
                          CachedModels.Add(ID, result);
                      }
                      else
                      {
                          result = null;
                      }
                  }
              }
          }

      return result;
      */
    }

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the sitemodel to be reloaded
    /// </summary>
    /// <param name="SiteModelID"></param>
    public void SiteModelAttributesHaveChanged(Guid SiteModelID)
    {
      // Remove or updtae if necessary the Sitemodel from any cached storage in this context
      //GetSiteModel(ImmutableStorageProxy, SiteModelID, false)?.LoadFromPersistentStore(ImmutableStorageProxy);
    }
  }
}
