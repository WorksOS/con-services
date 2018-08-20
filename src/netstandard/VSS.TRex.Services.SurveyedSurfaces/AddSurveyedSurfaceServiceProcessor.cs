using Apache.Ignite.Core.Cache;
using System;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.SurveyedSurfaces
{
    /// <summary>
    /// Service processor to handle adding a new surveyed surface to the list for a sitemodel
    /// </summary>
    [Serializable]
    public class AddSurveyedSurfaceProcessor : ICacheEntryProcessor<NonSpatialAffinityKey, byte[], ISurveyedSurface, bool>
    {
        public bool Process(IMutableCacheEntry<NonSpatialAffinityKey, byte[]> entry, ISurveyedSurface arg)
        {
            try
            {
                TRex.SurveyedSurfaces.SurveyedSurfaces ss = new TRex.SurveyedSurfaces.SurveyedSurfaces(); 
                if (entry.Exists)
                {
                    ss.FromBytes(entry.Value);
                }

                ss.AddSurveyedSurfaceDetails(arg.ID, arg.Get_DesignDescriptor(), arg.AsAtDate, arg.Extents);

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

