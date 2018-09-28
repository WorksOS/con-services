using Apache.Ignite.Core.Cache;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.SurveyedSurfaces
{
    /// <summary>
    /// Service processor to handle removing a surveyed surface from the list for a sitemodel
    /// </summary>
    [Serializable]
    public class RemoveSurveyedSurfaceProcessor : ICacheEntryProcessor<INonSpatialAffinityKey, byte[], Guid, bool>
    {
        public bool Process(IMutableCacheEntry<INonSpatialAffinityKey, byte[]> entry, Guid arg)
        {
            try
            {
                ISurveyedSurfaces ss = DIContext.Obtain<ISurveyedSurfaces>();
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
