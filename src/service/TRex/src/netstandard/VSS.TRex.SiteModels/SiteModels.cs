using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Caching.Interfaces;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModels>();

    /// <summary>
    /// The cached set of site models that are currently 'open' in TRex
    /// </summary>
    private readonly Dictionary<Guid, ISiteModel> CachedModels = new Dictionary<Guid, ISiteModel>();

    private IStorageProxy _StorageProxy;
    private readonly Func<IStorageProxy> StorageProxyFactory;

    /// <summary>
    /// The default storage proxy to be used for requests
    /// </summary>
    public IStorageProxy StorageProxy => _StorageProxy ?? (_StorageProxy = StorageProxyFactory());

    /// <summary>
    /// Default no-arg constructor. Made private to enforce provision of storage proxy
    /// </summary>
    private SiteModels() { }

    /// <summary>
    /// Constructs a SiteModels instance taking a storageProxyFactory delegate that will create the
    /// appropriate primary storage proxy
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
    /// Retrieves a site model from the persistent store ready for use. If the site model does not
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
          // the one just created in favor of the one in the dictionary
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
          // the one just created in favor of the one in the dictionary
          if (CachedModels.TryGetValue(id, out ISiteModel result2))
            return result2;

          Log.LogInformation($"Creating new site model {id} and adding to internal cache");

          CachedModels.Add(id, result);

          // Establish the metadata entry for this new site model
          DIContext.Obtain<ISiteModelMetadataManager>().Add(id, result.MetaData);

          return result;
        }
      }

      return null;
    }

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the site model to be reloaded
    /// </summary>
    /// <param name="message"></param>
    public void SiteModelAttributesHaveChanged(ISiteModelAttributesChangedEvent message)
    {
      // Site models have immutable characteristics in TRex. Multiple requests may reference the same site model
      // concurrently, with no interlocks enforcing access serialization. Any attempt to replace or modify an already loaded
      // site model may cause issue with concurrent request accessing that site model.
      // THe strategy here is to preserve continued access by concurrent requests to the site model retrieved
      // at the time the request was initiated by removing it from the SiteModels cache but not destroying it.
      // Once all request based references to the site model have completed the now orphaned site model will be cleaned
      // up by the garbage collector. Removal of the site model is interlocked with getting a site model reference
      // to ensure no concurrency issues within the underlying cache implementation
      // Note: The site model references some elements that may be preserved via the site model factory method that
      // accepts an origin site model.
      // These elements are:
      // 1. ExistenceMap
      // 2. Sub grid tree containing cached sub grid data
      // 3. Coordinate system
      // 4. Designs
      // 5. Surveyed Surfaces
      // 6. Machines
      // 7. Machines target values
      // 8. Machines design names

      ISiteModel siteModel;

      // Construct a new site model that preserves elements not affected by the notification and replace the existing 
      // site model reference with it.
      lock (CachedModels)
      {
        CachedModels.TryGetValue(message.SiteModelID, out siteModel);

        if (siteModel == null)
          return;

        // Note: The spatial data grid is highly conserved and never killed in a site model change notification.
        SiteModelOriginConstructionFlags originFlags =
          SiteModelOriginConstructionFlags.PreserveGrid
          | (!message.ExistenceMapModified ? SiteModelOriginConstructionFlags.PreserveExistenceMap : 0)
          | (!message.CsibModified ? SiteModelOriginConstructionFlags.PreserveCsib : 0)
          | (!message.DesignsModified ? SiteModelOriginConstructionFlags.PreserveDesigns : 0)
          | (!message.SurveyedSurfacesModified ? SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces : 0)
          | (!message.MachinesModified ? SiteModelOriginConstructionFlags.PreserveMachines : 0)
          | (!message.MachineTargetValuesModified ? SiteModelOriginConstructionFlags.PreserveMachineTargetValues : 0)
          | (!message.MachineDesignsModified ? SiteModelOriginConstructionFlags.PreserveMachineDesigns | SiteModelOriginConstructionFlags.PreserveSiteModelDesigns : 0)
          | (!message.ProofingRunsModified ? SiteModelOriginConstructionFlags.PreserveProofingRuns : 0)
          | (!message.AlignmentsModified ? SiteModelOriginConstructionFlags.PreserveAlignments : 0)
          ;

        Log.LogInformation($"Processing attribute change notification for site model {message.SiteModelID}. Preserved elements are {originFlags}");

        // First create a new site model to replace the site model with, requesting certain elements of the existing site model
        // to be preserved in the new site model instance.
        siteModel = DIContext.Obtain<ISiteModelFactory>().NewSiteModel(siteModel, originFlags);

        // Replace the site model reference in the cache with the new site model
        CachedModels[message.SiteModelID] = siteModel;
      }

      // If the notification contains an existence map change mask then all cached sub grid based elements that match the masked sub grids
      // need to be evicted from all cached contexts related to this site model. Note: This operation is not performed under a lock as the 
      // removal operations on the cache are lock free
      if (message.ExistenceMapChangeMask != null)
      {
        // Create and deserialize the sub grid but mask from the message
        ISubGridTreeBitMask mask = new SubGridTreeSubGridExistenceBitMask();
        mask.FromBytes(message.ExistenceMapChangeMask);

        // Iterate over all leaf sub grids in the mask. For each get the matching node sub grid in siteModel.Grid, 
        // and remove all sub grid references from that node sub grid matching the bits in the bit mask sub grid
        mask.ScanAllSubGrids(leaf =>
        {
          // Obtain the matching node sub grid in Grid
          ISubGrid node = siteModel.Grid.LocateClosestSubGridContaining
            (leaf.OriginX << SubGridTreeConsts.SubGridIndexBitsPerLevel,
             leaf.OriginY << SubGridTreeConsts.SubGridIndexBitsPerLevel,
             leaf.Level);

          // If there are sub grids present in Grid that match the sub grids identified by leaf
          // remove the elements identified in leaf from the node sub grid.
          if (node != null)
          {
            ((ISubGridTreeLeafBitmapSubGrid)leaf).ForEachSetBit((x, y) => node.SetSubGrid(x, y, null));
          }

          return true; // Keep processing leaf sub grids
        });

        // Advise the spatial memory general sub grid result cache of the change so it can invalidate cached derivatives
        DIContext.Obtain<ITRexSpatialMemoryCache>()?.InvalidateDueToProductionDataIngest(message.SiteModelID, mask);
      }
    }
  }
}
