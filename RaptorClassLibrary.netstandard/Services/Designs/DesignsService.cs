using Apache.Ignite.Core.Cache;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.Designs
{
    /// <summary>
    /// Service metaphor providing access andmanagement control over designs stored for site models
    /// </summary>
    public class DesignsService : BaseRaptorService, IDesignsService // , IService, 
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        [NonSerialized]
        private static DesignsService _instance;

        public static DesignsService Instance() 
        {
            if (_instance == null)
            {
                _instance = new DesignsService(StorageMutability.Immutable);
                _instance.Init();
            }

            return _instance;
        }

        /// <summary>
        /// Cache storing sitemodel instances
        /// </summary>
        private ICache<NonSpatialAffinityKey, byte[]> mutableNonSpatialCache;

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

        public void Init()
        {
            // Delegate to the service Init() method if this becomes an Ignite service
            mutableNonSpatialCache = _Ignite.GetCache<NonSpatialAffinityKey, byte[]>(CacheName);
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
        public void AddDirect(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, out long DesignID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

            NonSpatialAffinityKey cacheKey = TRex.Designs.Storage.Designs.CacheKey(SiteModelID);
            DesignID = Guid.NewGuid().GetHashCode();

            // Get the designs, creating it if it does not exist
            TRex.Designs.Storage.Designs designList = new TRex.Designs.Storage.Designs();
            try
            {
                designList.FromBytes(mutableNonSpatialCache.Get(cacheKey));
            }
            catch (KeyNotFoundException)
            {
                // Swallow exception, the list will be empty
            }

            // Add the new design, generating a random ID from a GUID
            designList.AddDesignDetails(DesignID, designDescriptor, extents);

            // Put the list back into the cache with the new entry
            mutableNonSpatialCache.Put(cacheKey, designList.ToBytes());
        }

        public TRex.Designs.Storage.Designs List(Guid SiteModelID)
        {
            Log.InfoFormat($"Listing designs from {TRex.Designs.Storage.Designs.CacheKey(SiteModelID)}");

            try
            {
                TRex.Designs.Storage.Designs designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(mutableNonSpatialCache.Get(TRex.Designs.Storage.Designs.CacheKey(SiteModelID)));

                return designList;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public TRex.Designs.Storage.Designs ListDirect(Guid SiteModelID) => List(SiteModelID);

        /*
            /// <summary>
            /// Defines the actions to take if the service is cancelled
            /// </summary>
            /// <param name="context"></param>
            public void Cancel(IServiceContext context)
            {
                mutableNonSpatialCache.Remove(_svcName);
            }

            /// <summary>
            /// Defines the actions to take when the service is first executed
            /// </summary>
            /// <param name="context"></param>
            public void Execute(IServiceContext context)
            {
                Log.Info($"Executing Raptor Service 'Designs'");
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

                mutableNonSpatialCache = _ignite.GetCache<NonSpatialAffinityKey, Byte[]>(CacheName /*TRexCaches.MutableNonSpatialCacheName());
    */

        public bool Remove(Guid SiteModelID, long DesignID) => RemoveDirect(SiteModelID, DesignID);
        
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
               return mutableNonSpatialCache.Invoke(Raptor.Designs.Storage.Designs.CacheKey(SiteModelID),
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
        public bool RemoveDirect(Guid SiteModelID, long DesignID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition
            try
            {
                NonSpatialAffinityKey cacheKey = TRex.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                TRex.Designs.Storage.Designs designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(mutableNonSpatialCache.Get(cacheKey));

                // Remove the design
                bool result = designList.RemoveDesign(DesignID);

                // Put the list back into the cache with the new entry
                if (result)
                {
                    mutableNonSpatialCache.Put(cacheKey, designList.ToBytes());
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
        public Design Find(Guid SiteModelID, long DesignID) => FindDirect(SiteModelID, DesignID);

        /// <summary>
        /// Finds a given design in a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        public Design FindDirect(Guid SiteModelID, long DesignID)
        {
            try
            {
                NonSpatialAffinityKey cacheKey = TRex.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                TRex.Designs.Storage.Designs designList = new TRex.Designs.Storage.Designs();
                designList.FromBytes(mutableNonSpatialCache.Get(cacheKey));

                // Find the design and return it
                return designList.Count == 0 ? null : designList.Find(x => x.ID == DesignID);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}

