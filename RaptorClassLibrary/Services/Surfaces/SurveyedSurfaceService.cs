using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Resource;
using Apache.Ignite.Core.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
//using VSS.VisionLink.Raptor.GridFabric.Caches;
//using VSS.VisionLink.Raptor.GridFabric.Grids;
//using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Surfaces;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{

    /// <summary>
    /// A test of how to add a new surveyed surface
    /// </summary>
    public class SurveyedSurfaceService : BaseRaptorService, IService, ISurveyedSurfaceService
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Cache storing sitemodel instances
        /// </summary>
        private ICache<String, Byte[]> mutableNonSpatialCache;

        /// <summary>
        /// Service name.
        /// </summary>
        private string _svcName;

        private string GridName;
        private string CacheName;

        public SurveyedSurfaceService()
        {

        }

        public SurveyedSurfaceService(string gridName, string cacheName) : base()
        {
            GridName = gridName;
            CacheName = cacheName;
        }

        /// <summary>
        /// Add a new surveyed surface to a sitemodel
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="AsAtDate"></param>
        public void Add(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents )
        {
            mutableNonSpatialCache.Invoke(SurveyedSurfaces.CacheKey(SiteModelID), 
                                          new AddSurveyedSurfaceProcessor(), 
                                          new SurveyedSurface(SiteModelID, designDescriptor, asAtDate, extents));
        }

        /// <summary>
        /// Add a new surveyed surface to a sitemodel
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="AsAtDate"></param>
        public void AddDirect(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents)
        {
            try
            {
                // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

                string cacheKey = SurveyedSurfaces.CacheKey(SiteModelID);

                // Get the surveyed surfaces, creating it if it does not exist
                SurveyedSurfaces ssList = SurveyedSurfaces.FromBytes(mutableNonSpatialCache.Get(cacheKey));

                if (ssList == null)
                {
                    ssList = new SurveyedSurfaces();
                }

                // Add the new surveyed surface, generating a random ID from a GUID
                SurveyedSurface ss = ssList.AddSurveyedSurfaceDetails(Guid.NewGuid().GetHashCode(), designDescriptor, asAtDate, extents);

                // Put the list back into the cache with the new entry
                mutableNonSpatialCache.Put(cacheKey, ssList.ToByteArray());
            }
            catch (KeyNotFoundException)
            {
                // Swallow exception
            }
        }

        public SurveyedSurfaces List(long SiteModelID)
        {
            Log.InfoFormat($"Listing surveyed surfaces from {SurveyedSurfaces.CacheKey(SiteModelID)}");

            try
            {
                return SurveyedSurfaces.FromBytes(mutableNonSpatialCache.Get(SurveyedSurfaces.CacheKey(SiteModelID)));
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public SurveyedSurfaces ListDirect(long SiteModelID) => List(SiteModelID);

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
           Log.Info($"Executing Raptor Service 'AddSurveyedSurface'");
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

//            if (_ignite == null)
//            {
//                _ignite = Ignition.TryGetIgnite(GridName);
//            }

            mutableNonSpatialCache = _ignite.GetCache<String, Byte[]>(CacheName /*RaptorCaches.MutableNonSpatialCacheName()*/);
        }

        /// <summary>
        /// Remove a given surveyed surface from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="SurveySurfaceID"></param>
        /// <returns></returns>
        public bool Remove(long SiteModelID, long SurveySurfaceID)
        {
            try
            {
                return mutableNonSpatialCache.Invoke(SurveyedSurfaces.CacheKey(SiteModelID),
                                                     new RemoveSurveyedSurfaceProcessor(),
                                                     SurveySurfaceID);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove a given surveyed surface from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="SurveySurfaceID"></param>
        /// <returns></returns>
        public bool RemoveDirect(long SiteModelID, long SurveySurfaceID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

            try
            {
                string cacheKey = SurveyedSurfaces.CacheKey(SiteModelID);

                // Get the surveyed surfaces, creating it if it does not exist
                SurveyedSurfaces ssList = SurveyedSurfaces.FromBytes(mutableNonSpatialCache.Get(cacheKey));

                if (ssList == null)
                {
                    ssList = new SurveyedSurfaces();
                }

                // Add the new surveyed surface, generating a random ID from a GUID
                bool result = ssList.RemoveSurveyedSurface(SurveySurfaceID);

                // Put the list back into the cache with the new entry
                if (result)
                {
                    mutableNonSpatialCache.Put(cacheKey, ssList.ToByteArray());
                }

                return result;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
    }
}
