using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.Designs
{
    /// <summary>
    /// Service metaphor providing access and management control over designs stored for site models
    /// </summary>
    public class DesignsService : BaseService, IDesignsService 
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Cache storing sitemodel instances
        /// </summary>
        private ICache<INonSpatialAffinityKey, byte[]> mutableNonSpatialCache;

        private ICache<INonSpatialAffinityKey, byte[]> MutableNonSpatialCache => mutableNonSpatialCache ?? (mutableNonSpatialCache = _Ignite.GetCache<INonSpatialAffinityKey, byte[]>(CacheName));

        /// <summary>
        /// Service name.
        /// </summary>
    //private string _svcName;

        private string CacheName;

        /// <summary>
        /// Default no-arg constructor that sets the grid and cache name to default values
        /// </summary>
        public DesignsService(StorageMutability Mutability) : base(TRexGrids.GridName(Mutability), "DesignsService")
        {
            CacheName = TRexCaches.ImmutableNonSpatialCacheName();
        }

        public DesignsService(string cacheName) : this(StorageMutability.Immutable)
        {
            CacheName = cacheName;
        }

        public void Add(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
        {
            throw new NotImplementedException();
        }

        /*
            /// <summary>
            /// Add a new design to a sitemodel
            /// </summary>
            /// <param name="SiteModelID"></param>
            /// <param name="designDescriptor"></param>
            public void Add(long SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
            {
                mutableNonSpatialCache.Invoke(CacheKey(SiteModelID),
                                              new AddDesignProcessor(),
                                              new Design(SiteModelID, designDescriptor, extents));
            }
*/

        /// <summary>
        /// Add a new design to a sitemodel
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="extents"></param>
        /// <param name="DesignID"></param>
        public void AddDirect(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, out Guid DesignID)
        {
            // This should be done under a lock on the cache key. For now, we will live with the race condition...

            INonSpatialAffinityKey cacheKey = VSS.TRex.Designs.Storage.Designs.CacheKey(SiteModelID);
            DesignID = Guid.NewGuid();

            // Get the designs, creating it if it does not exist
            IDesigns designList = new TRex.Designs.Storage.Designs();
            try
            {
                designList.FromBytes(MutableNonSpatialCache.Get(cacheKey));
            }
            catch (KeyNotFoundException)
            {
                // Swallow exception, the list will be empty
            }

            // Add the new design, generating a random ID from a GUID
            designList.AddDesignDetails(DesignID, designDescriptor, extents);

            // Put the list back into the cache with the new entry
            MutableNonSpatialCache.Put(cacheKey, designList.ToBytes());
        }

        public IDesigns List(Guid SiteModelID)
        {
            Log.LogInformation($"Listing designs from {TRex.Designs.Storage.Designs.CacheKey(SiteModelID)}");

            try
            {
                IDesigns designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(MutableNonSpatialCache.Get(TRex.Designs.Storage.Designs.CacheKey(SiteModelID)));

                return designList;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public IDesigns ListDirect(Guid SiteModelID) => List(SiteModelID);

        /*
            /// <summary>
            /// Defines the actions to take if the service is cancelled
            /// </summary>
            /// <param name="context"></param>
            public void Cancel(IServiceContext context)
            {
                MutableNonSpatialCache.Remove(_svcName);
            }

            /// <summary>
            /// Defines the actions to take when the service is first executed
            /// </summary>
            /// <param name="context"></param>
            public void Execute(IServiceContext context)
            {
                Log.LogInformation($"Executing TRex Service 'Designs'");
            }

            /// <summary>
            /// Defines the actions to take when the service is initialised prior to execution
            /// </summary>
            /// <param name="context"></param>
            public void Init(IServiceContext context)
            {
                if (context != null)
                {
                    _svcName = context.Name;
                }

                MutableNonSpatialCache = _ignite.GetCache<INonSpatialAffinityKey, Byte[]>(CacheName /*TRexCaches.MutableNonSpatialCacheName());
    */

        public bool Remove(Guid SiteModelID, Guid DesignID) => RemoveDirect(SiteModelID, DesignID);
        
        /*
       /// <summary>
       /// Remove a given design from a site model
       /// </summary>
       /// <param name="SiteModelID"></param>
       /// <param name="DesignID"></param>
       /// <returns></returns>
       public bool Remove(long SiteModelID, long DesignID)
       {
           try
           {
               return MutableNonSpatialCache.Invoke(TRex.Designs.Storage.Designs.CacheKey(SiteModelID),
                                                    new RemoveDesignProcessor(),
                                                    DesignID);
           }
           catch (KeyNotFoundException)
           {
               return false;
           }
       }
        */

        /// <summary>
        /// Remove a given design from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        public bool RemoveDirect(Guid SiteModelID, Guid DesignID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition
            try
            {
                INonSpatialAffinityKey cacheKey = TRex.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                IDesigns designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(MutableNonSpatialCache.Get(cacheKey));

                // Remove the design
                bool result = designList.RemoveDesign(DesignID);

                // Put the list back into the cache with the new entry
                if (result)
                {
                    MutableNonSpatialCache.Put(cacheKey, designList.ToBytes());
                }

                return result;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Finds a given design in a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        public IDesign Find(Guid SiteModelID, Guid DesignID) => FindDirect(SiteModelID, DesignID);

        /// <summary>
        /// Finds a given design in a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        public IDesign FindDirect(Guid SiteModelID, Guid DesignID)
        {
            try
            {
                INonSpatialAffinityKey cacheKey = TRex.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                IDesigns designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(MutableNonSpatialCache.Get(cacheKey));

                // Find the design and return it
                return designList.Count == 0 ? null : designList.Locate(DesignID);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}

