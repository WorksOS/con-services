using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using VSS.TRex.DI;
using VSS.TRex.Exceptions;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Provides a manager abstraction for accessing and maintaining site model metadata
  /// </summary>
  public class SiteModelMetadataManager : ISiteModelMetadataManager
  {
    /// <summary>
    /// The Ignite cache containing the sitemodel metadata
    /// </summary>
    private ICache<Guid, ISiteModelMetadata> metaDataCache;

    /// <summary>
    /// Configure the parameters of the existence map cache
    /// </summary>
    public CacheConfiguration ConfigureCache()
    {
      return new CacheConfiguration()
      {
        Name = TRexCaches.SiteModelMetadataCacheName(),

        // cfg.CopyOnRead = false;   Leave as default as should have no effect with 2.1+ without on heap caching enabled
        KeepBinaryInStore = false,

        // Replicate the sitemodel metadata across nodes
        CacheMode = CacheMode.Replicated,

        // No backups for now
        Backups = 0,

        DataRegionName = DataRegions.DEFAULT_DATA_REGION_NAME
      };
    }

    /// <summary>
    /// Constructs a site model meta data manager instance oriented to the TRex grid that is the primary grid
    /// referenced by the DI'c SiteModels instance
    /// </summary>
    public SiteModelMetadataManager()
    {
      // Obtain the ignite reference for the primary grid orientation of SiteModels
      IIgnite ignite = DIContext.Obtain<ITRexGridFactory>().Grid(DIContext.Obtain<ISiteModels>().StorageProxy.Mutability);

      metaDataCache = ignite.GetOrCreateCache<Guid, ISiteModelMetadata>(ConfigureCache());
    }

    /// <summary>
    /// Adds a new metadata record for a sitemodel
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="metaData"></param>
    public void Add(Guid siteModelID, ISiteModelMetadata metaData)
    {
        metaDataCache.Put(siteModelID, metaData);
    }

    /// <summary>
    /// Requests TRex update the stored metadata for a particular sitemodel by creating a new
    /// metadata record from cratch.
    /// Note: This does force the site model to load a range of elements to construct the update
    /// metadata object.
    /// </summary>
    /// <param name="siteModelID"></param>
    public void Update(Guid siteModelID)
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelID);

      if (siteModel != null)
        metaDataCache.Put(siteModelID, siteModel.MetaData);
    }

    /// <summary>
    /// Updates a metadata record for a site model given a pre-built metadata instance
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="metaData"></param>
    public void Update(Guid siteModelID, ISiteModelMetadata metaData)
    {
        metaDataCache.Put(siteModelID, metaData);
    }

    /// <summary>
    /// Retrieves the metadata for a single site model
    /// </summary>
    /// <param name="siteModel"></param>
    /// <returns></returns>
    public ISiteModelMetadata Get(Guid siteModel)
    {
      try
      {
        return metaDataCache.Get(siteModel);
      }
      catch (KeyNotFoundException)
      {
        return null;
      }
      catch (Exception e)
      {
        throw new TRexException($"Failure to retrieve metadata for site model {siteModel}", e);
      }
    }

    /// <summary>
    /// Retrieves the metadata for all site models (warning: this may be large!)
    /// </summary>
    /// <returns></returns>
    public ISiteModelMetadata[] GetAll()
    {
      try
      {
        return metaDataCache.Query(new ScanQuery<Guid, SiteModelMetadata>()).GetAll().Select(x => x.Value).ToArray();
      }
      catch (Exception e)
      {
        throw new TRexException($"Failure to retrieve all metadata for site models", e);
      }
    }
  }
}
