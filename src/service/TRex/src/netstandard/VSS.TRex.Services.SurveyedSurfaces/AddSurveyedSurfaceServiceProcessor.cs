using Apache.Ignite.Core.Cache;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Services.SurveyedSurfaces
{
    /// <summary>
    /// Service processor to handle adding a new surveyed surface to the list for a sitemodel
    /// </summary>
    public class AddSurveyedSurfaceProcessor : ICacheEntryProcessor<INonSpatialAffinityKey, byte[], ISurveyedSurface, bool>
    {
        public bool Process(IMutableCacheEntry<INonSpatialAffinityKey, byte[]> entry, ISurveyedSurface arg)
        {
            try
            {
                ISurveyedSurfaces ss = DIContext.Obtain<ISurveyedSurfaces>();
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

