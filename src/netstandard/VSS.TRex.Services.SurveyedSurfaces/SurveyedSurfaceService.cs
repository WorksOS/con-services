using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.Services.Surfaces;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.SurveyedSurfaces
{

    /// <summary>
    /// A test of how to manage surveyed surfaces
    /// </summary>
    public class SurveyedSurfaceService : BaseService, IService, ISurveyedSurfaceService
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Cache storing sitemodel instances
        /// </summary>
        private ICache<NonSpatialAffinityKey, byte[]> mutableNonSpatialCache;

        /// <summary>
        /// Service name.
        /// </summary>
        private string _svcName;

        private string CacheName;

        /// <summary>
        /// Default no-arg constructor supplied default TRex grid and MutableNonSpatial cache name for surveyed surface information
        /// </summary>
        public SurveyedSurfaceService(StorageMutability mutability) : base(TRexGrids.GridName(mutability), "SurveyedSurfaceService")
        {
            CacheName = TRexCaches.ImmutableNonSpatialCacheName();
        }

        public SurveyedSurfaceService(StorageMutability mutability, string cacheName) : this(mutability)
        {
            CacheName = cacheName;
        }

        /// <summary>
        /// Add a new surveyed surface to a sitemodel
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        /// <param name="extents"></param>
        public void Add(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents )
        {
            mutableNonSpatialCache.Invoke(TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID), 
                                          new AddSurveyedSurfaceProcessor(), 
                                          new SurveyedSurface(designDescriptor.DesignID, designDescriptor, asAtDate, extents));
        }

        /// <summary>
        /// Add a new surveyed surface to a sitemodel via direct manipulation of the information in the grid
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="asAtDate"></param>
        /// <param name="extents"></param>
        /// <param name="SuveyedSurfaceID"></param>
        public void AddDirect(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents, out Guid SuveyedSurfaceID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

            NonSpatialAffinityKey cacheKey = TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID);
            SuveyedSurfaceID = Guid.NewGuid();

            // Get the surveyed surfaces, creating it if it does not exist
            TRex.SurveyedSurfaces.SurveyedSurfaces ssList = new TRex.SurveyedSurfaces.SurveyedSurfaces();

            try
            {
                ssList.FromBytes(mutableNonSpatialCache.Get(cacheKey));
            }
            catch (KeyNotFoundException)
            {
                // Swallow exception, the list will be empty
            }
            catch
            {
                throw;
            }

            // Add the new surveyed surface, generating a random ID from a GUID
            ssList.AddSurveyedSurfaceDetails(SuveyedSurfaceID, designDescriptor, asAtDate, extents);

            // Put the list back into the cache with the new entry
            mutableNonSpatialCache.Put(cacheKey, ssList.ToBytes());
        }

        /// <summary>
        /// List the surveyed surfaces for a site model
        /// </summary>
        public ISurveyedSurfaces List(Guid SiteModelID)
        {
            Log.LogInformation($"Listing surveyed surfaces from {TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID)}");

            try
            {
                TRex.SurveyedSurfaces.SurveyedSurfaces ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
                ss.FromBytes(mutableNonSpatialCache.Get(TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID)));
                return ss;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// List the surveyed surfaces for a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        public ISurveyedSurfaces ListDirect(Guid SiteModelID) => List(SiteModelID);

        /// <summary>
        /// Defines the actions to take if the service is cancelled
        /// </summary>
        /// <param name="context"></param>
        public void Cancel(IServiceContext context)
        {
        }

        /// <summary>
        /// Defines the actions to take when the service is first executed
        /// </summary>
        /// <param name="context"></param>
        public void Execute(IServiceContext context)
        {
           Log.LogInformation("Executing TRex Service 'SurveyedSurfaceService'");
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

            mutableNonSpatialCache = _Ignite.GetCache<NonSpatialAffinityKey, byte[]>(CacheName);
        }

        /// <summary>
        /// Remove a given surveyed surface from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="SurveySurfaceID"></param>
        /// <returns></returns>
        public bool Remove(Guid SiteModelID, Guid SurveySurfaceID)
        {
            try
            {
                return mutableNonSpatialCache.Invoke(TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID),
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
        public bool RemoveDirect(Guid SiteModelID, Guid SurveySurfaceID)
        {
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

            try
            {
                NonSpatialAffinityKey cacheKey = TRex.SurveyedSurfaces.SurveyedSurfaces.CacheKey(SiteModelID);

                // Get the surveyed surfaces, creating it if it does not exist
                TRex.SurveyedSurfaces.SurveyedSurfaces ssList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
                ssList.FromBytes(mutableNonSpatialCache.Get(cacheKey));

                if (ssList == null)
                {
                    ssList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
                }

                // Add the new surveyed surface, generating a random ID from a GUID
                bool result = ssList.RemoveSurveyedSurface(SurveySurfaceID);

                // Put the list back into the cache with the new entry
                if (result)
                {
                    mutableNonSpatialCache.Put(cacheKey, ssList.ToBytes());
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
