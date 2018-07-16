using Apache.Ignite.Core.Cache;
using System;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Surfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.Surfaces
{
    /// <summary>
    /// Service processor to handle adding a new surveyed surface to the list for a sitemodel
    /// </summary>
    [Serializable]
    public class AddSurveyedSurfaceProcessor : ICacheEntryProcessor<NonSpatialAffinityKey, byte[], SurveyedSurface, bool>
    {
        public bool Process(IMutableCacheEntry<NonSpatialAffinityKey, byte[]> entry, SurveyedSurface arg)
        {
            try
            {
                SurveyedSurfaces ss = new SurveyedSurfaces(); 
                if (entry.Exists)
                {
                    ss.FromBytes(entry.Value);
                }

                ss.AddSurveyedSurfaceDetails(arg.ID, arg.DesignDescriptor, arg.AsAtDate, arg.Extents);

                entry.Value = ss.ToBytes();

                return true;
            }
            catch
            {
                throw; // return false;
            }
        }
    }
}

