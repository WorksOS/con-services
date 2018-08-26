using System.Collections.Generic;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;

namespace VSS.TRex.SurveyedSurfaces
{
    /// <summary>
    /// Contains a cache of previously requested surveyed surface elevation results. This cache is simple in that no cap or retirement policy is implemented
    /// for elements within it.
    /// </summary>
    public class SurveyedSurfaceResultCache
    {
        /// <summary>
        /// The internal store of previously requested subgrids of surveyed surface information
        /// </summary>
        private Dictionary<string, ClientHeightAndTimeLeafSubGrid> FCache = new Dictionary<string, ClientHeightAndTimeLeafSubGrid>();

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurveyedSurfaceResultCache()
        {

        }

        /// <summary>
        /// Gets an item from the cache, returning null if it is not present
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public ClientHeightAndTimeLeafSubGrid Get(SurfaceElevationPatchArgument arg)
        {
            lock (FCache)
            {
                return FCache.TryGetValue(arg.CacheKey(), out ClientHeightAndTimeLeafSubGrid result) ? result : null;
            }
        }

        /// <summary>
        /// Puts an item into the cache, returning true if it was added, false if there was already an entry for the key
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Put(SurfaceElevationPatchArgument arg, ClientHeightAndTimeLeafSubGrid value)
        {
            lock (FCache)
            {
                string key = arg.CacheKey();

                if (FCache.ContainsKey(key))
                {
                    return false;
                }

                FCache.Add(key, value);

                return true;
            }
        }
    }
}
