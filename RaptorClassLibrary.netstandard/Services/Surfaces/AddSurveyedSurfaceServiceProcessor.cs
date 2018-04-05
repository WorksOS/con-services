using Apache.Ignite.Core.Cache;
using System;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.Utilities.ExtensionMethods;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Service processor to handle adding a new surveyed surface to the list for a sitemodel
    /// </summary>
    [Serializable]
    public class AddSurveyedSurfaceProcessor : ICacheEntryProcessor<string, byte[], SurveyedSurface, bool>
    {
        public bool Process(IMutableCacheEntry<string, byte[]> entry, SurveyedSurface arg)
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

