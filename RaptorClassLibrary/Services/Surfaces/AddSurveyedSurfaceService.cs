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
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Surfaces;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{

    /// <summary>
    /// A test of how to add a new surveyed surface
    /// </summary>
    public class AddSurveyedSurfaceService : IService, IAddSurveyedSurfaceService
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Injected Ignite instance
        /// </summary>
        [InstanceResource]
        private readonly IIgnite _ignite;

        /// <summary>
        /// Cache storing sitemodel instances
        /// </summary>
        private ICache<String, Byte[]> mutableNonSpatialCache;

        /// <summary>
        /// Service name.
        /// </summary>
        private string _svcName;

        /// <summary>
        /// Add a new surveyed surface to a sitemodel
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="AsAtDate"></param>
        public void Add(long SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate)
        {
            mutableNonSpatialCache.Invoke(SurveyedSurfaces.CacheKey(SiteModelID), 
                                          new AddSurveyedSurfaceProcessor(), 
                                          new SurveyedSurface(SiteModelID, designDescriptor, asAtDate));

            /*
            // TODO: This should be done under a lock on the cache key. For now, we will live with the race condition

                        // Get the surveyed surfaces, creating it if it does not exist
                        SurveyedSurfaces SurveyedSurfaces = SurveyedSurfaces.FromBytes(mutableNonSpatialCache.Get(SurveyedSurfaces.CacheKey(SiteModelID))) ?? new SurveyedSurfaces();

                        // Add the new surveyed surface, generating a random ID from a GUID
                        SurveyedSurface ss = SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid().GetHashCode(), designDescriptor, asAtDate);

                        // Put the list back into the cache with the new entry
                        mutableNonSpatialCache.Put(SurveyedSurfaces.CacheKey(SiteModelID), SurveyedSurfaces.ToByteArray());
            */
        }

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
            Log.Info($"Executing Raptor Service 'AddSurveyedSurface': {this}");
        }

        /// <summary>
        /// Defines the actions to take when the service is initialised prior to execution
        /// </summary>
        /// <param name="context"></param>
        public void Init(IServiceContext context)
        {
            _svcName = context.Name;

            mutableNonSpatialCache = _ignite.GetCache<String, Byte[]>(RaptorCaches.MutableNonSpatialCacheName());
        }
    }
}
