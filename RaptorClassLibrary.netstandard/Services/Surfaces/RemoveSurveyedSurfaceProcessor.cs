using Apache.Ignite.Core.Cache;
using System;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.Utilities.ExtensionMethods;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Service processor to handle removing a surveyed surface from the list for a sitemodel
    /// </summary>
    [Serializable]
    public class RemoveSurveyedSurfaceProcessor : ICacheEntryProcessor<NonSpatialAffinityKey, byte[], long, bool>
    {
        public bool Process(IMutableCacheEntry<NonSpatialAffinityKey, byte[]> entry, long arg)
        {
            try
            {
                SurveyedSurfaces ss = new SurveyedSurfaces();
                if (entry.Exists)
                {
                    ss.FromBytes(entry.Value);
                }

                if (ss.RemoveSurveyedSurface(arg))
                {
                    entry.Value = ss.ToBytes();
                    return true;
                }

                return false;
            }
            catch
            {
                throw; // return false;
            }
        }
    }
}
