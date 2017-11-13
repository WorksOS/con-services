using Apache.Ignite.Core.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Surfaces;

namespace VSS.VisionLink.Raptor.Services.Surfaces
{
    /// <summary>
    /// Service processor to handle removing a surveyed surface from the list for a sitemodel
    /// </summary>
    [Serializable]
    public class RemoveSurveyedSurfaceProcessor : ICacheEntryProcessor<string, byte[], long, bool>
    {
        public bool Process(IMutableCacheEntry<string, byte[]> entry, long arg)
        {
            try
            {
                SurveyedSurfaces ss = entry.Exists ? SurveyedSurfaces.FromBytes(entry.Value) : null;

                if (ss!= null && ss.RemoveSurveyedSurface(arg))
                {
                    entry.Value = ss.ToByteArray();
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
