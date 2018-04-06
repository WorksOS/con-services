using Apache.Ignite.Core.Cache;
using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Utilities.ExtensionMethods;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.Services.Designs
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
        private ICache<string, byte[]> mutableNonSpatialCache;

        /// <summary>
        /// Service name.
        /// </summary>
        //private string _svcName;

        private string CacheName;

        /// <summary>
        /// Default no-arg constructor that sets the grid and cache name to default values
        /// </summary>
        public DesignsService(StorageMutability Mutability) : base(RaptorGrids.RaptorGridName(Mutability), "DesignsService")
        {
            CacheName = RaptorCaches.ImmutableNonSpatialCacheName();
        }

        public DesignsService(string cacheName) : this(StorageMutability.Immutable)
        {
            CacheName = cacheName;
        }

        public void Init()
        {
            // Delegate to the service Init() method if this becomes an Ignite service
            mutableNonSpatialCache = _Ignite.GetCache<string, byte[]>(CacheName);
        }

        public void Add(long SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
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
        public void AddDirect(long SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, out long DesignID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

            string cacheKey = Raptor.Designs.Storage.Designs.CacheKey(SiteModelID);
            DesignID = Guid.NewGuid().GetHashCode();

            // Get the designs, creating it if it does not exist
            Raptor.Designs.Storage.Designs designList = new Raptor.Designs.Storage.Designs();
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

        public Raptor.Designs.Storage.Designs List(long SiteModelID)
        {
            Log.InfoFormat($"Listing designs from {Raptor.Designs.Storage.Designs.CacheKey(SiteModelID)}");

            try
            {
                Raptor.Designs.Storage.Designs designList = new Raptor.Designs.Storage.Designs();
                designList.FromBytes(mutableNonSpatialCache.Get(Raptor.Designs.Storage.Designs.CacheKey(SiteModelID)));

                return designList;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public Raptor.Designs.Storage.Designs ListDirect(long SiteModelID) => List(SiteModelID);

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

                mutableNonSpatialCache = _ignite.GetCache<string, Byte[]>(CacheName /*RaptorCaches.MutableNonSpatialCacheName());
    */

        public bool Remove(long SiteModelID, long DesignID) => RemoveDirect(SiteModelID, DesignID);
        
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
        public bool RemoveDirect(long SiteModelID, long DesignID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition
            try
            {
                string cacheKey = Raptor.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                Raptor.Designs.Storage.Designs designList = new Raptor.Designs.Storage.Designs();
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
        public Raptor.Designs.Storage.Design Find(long SiteModelID, long DesignID) => FindDirect(SiteModelID, DesignID);

        /// <summary>
        /// Finds a given design in a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        public Raptor.Designs.Storage.Design FindDirect(long SiteModelID, long DesignID)
        {
            try
            {
                string cacheKey = Raptor.Designs.Storage.Designs.CacheKey(SiteModelID);

                // Get the designs, creating it if it does not exist
                Raptor.Designs.Storage.Designs designList = new Raptor.Designs.Storage.Designs();
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

