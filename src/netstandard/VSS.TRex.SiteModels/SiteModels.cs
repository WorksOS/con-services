using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
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

      if (result.LoadFromPersistentStore() == FileSystemErrorStatus.OK)
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

          // Establish the metadata entry for this new sitemodel
          DIContext.Obtain<ISiteModelMetadataManager>().Add(id, result.MetaData);

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
    /// <param name="message"></param>
    public void SiteModelAttributesHaveChanged(Guid SiteModelID, ISiteModelAttributesChangedEvent message)
    {
      // Sitemodels have immutable characteristics in TRex. Multiple requests may reference the same site model
      // concurrently, with no interlocks enforcing access serialisation. Any attempt to replace or modify an already loaded
      // sitemodel may cause issue with concurrent request accessing that site model.
      // THe strategy here is to preserve continued access by concurrent requests to the sitemodel retrieved
      // at the time the request was initiated by removing it from the SiteModels cache but not destroying it.
      // Once all request based references to the sitemodel have completed the now orphaned sitemodel will be cleaned
      // up by the garbage collector. Removal of the sitemodel is interlocked with getting a sitemodel reference
      // to ensure no concurrency issues within the underlying cache implementation
      // Note: The sitemodel references some elements that may be preserved via the site model factory method that
      // accepts an origin sitemodel.
      // These elements are:
      // 1. ExistenceMap
      // 2. Subgridtree containing cached subgrid data
      // 3. Coordinate system
      // 4. Designs
      // 5. Surveyed Surfaces
      // 6. Machines
      // 7. Machines target values

      ISiteModel siteModel;

      // Construct a new sitemodel that preserves elements not affected by the notification and replace the existing 
      // site model reference with it.
      lock (CachedModels)
      {
        CachedModels.TryGetValue(SiteModelID, out siteModel);

        if (siteModel == null)
          return;

        // Note: The spatial data grid is highly conserved and never killed in a sitemodel change notifiation.
        SiteModelOriginConstructionFlags originFlags = 
          SiteModelOriginConstructionFlags.PreserveGrid 
          | (!message.ExistenceMapModified ? SiteModelOriginConstructionFlags.PreserveExistenceMap : 0)
          | (!message.CsibModified ? SiteModelOriginConstructionFlags.PreserveCsib : 0)
          | (!message.DesignsModified ? SiteModelOriginConstructionFlags.PreserveDesigns : 0)
          | (!message.SurveyedSurfacesModified ? SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces : 0)
          | (!message.MachinesModified ? SiteModelOriginConstructionFlags.PreserveMachines : 0)
          | (!message.MachineTargetValuesModified? SiteModelOriginConstructionFlags.PreserveMachineTargetValues : 0);

        Log.LogInformation($"Processing attribute change notification for sitemodel {SiteModelID}. Preserved elements are {originFlags}");

        // First create a new sitemodel to replace the site model with, requesting certain elements of the existing sitemodel
        // to be preserved in the new sitemodel instance.
        ISiteModel newSiteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel(siteModel, originFlags);

        // Replace the site model reference in the cache with the new site model
        CachedModels[SiteModelID] = newSiteModel;
        siteModel = newSiteModel;
      }

      // If the notification contains an exeistance map change mask then all cached subgrid based elements that match the masked subgrids
      // need to be evicted from all cached contexts related to this sitemodel. Note: This operation is not performed under a lock as the 
      // removal operations on the cache are lock free
      // [todo: validate lock free operations on general result and similar cache stores as implemented]
      if (message.ExistenceMapChangeMask != null)
      {
        // Get the site model reference in case there was not a need to replace per the logic above
        siteModel = siteModel ?? CachedModels[SiteModelID];

        // Create and deserialise the subgrid but mask fron the message
        ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();
        mask.FromBytes(message.ExistenceMapChangeMask);
        
        // Iterate over all leaf subgrids in the mask. For each get the matching node subgrid in sitgeModel.Grid, 
        // and remove all subgrid references from that node subgrid matching by the bits in the bit mask subgrid
        mask.ScanAllSubGrids(leaf => 
        {
          // Obtain the matching node subgrid in Grid
          ISubGrid node = siteModel.Grid.LocateClosestSubGridContaining(leaf.OriginX, leaf.OriginY, leaf.Level);

          if (node != null) // There are subgrids present in Grid that match the subgrids identified by leaf
          {
            // Remove the elements identified in leaf from the node subgrid
            ((ISubGridTreeLeafBitmapSubGrid) leaf).ForEachSetBit((x, y) => node.SetSubGrid(x, y, null));

            // Remove other cache elements dependent on these subgrids
            // Todo... Remove other cache elements dependent on these subgrids
          }

          return true; // Keep processing leaf subgrids
        });      
      }   
    }
  }
}
