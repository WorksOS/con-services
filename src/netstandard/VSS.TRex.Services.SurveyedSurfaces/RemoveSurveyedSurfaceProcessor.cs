using Apache.Ignite.Core.Cache;
using System;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.SurveyedSurfaces
{
    /// <summary>
    /// Service processor to handle removing a surveyed surface from the list for a sitemodel
    /// </summary>
    [Serializable]
    public class RemoveSurveyedSurfaceProcessor : ICacheEntryProcessor<NonSpatialAffinityKey, byte[], Guid, bool>
    {
        public bool Process(IMutableCacheEntry<NonSpatialAffinityKey, byte[]> entry, Guid arg)
        {
            try
            {
                TRex.SurveyedSurfaces.SurveyedSurfaces ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
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
