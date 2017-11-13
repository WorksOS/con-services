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
    /// Service processor to handle adding a new survyedd surface to the list for a sitemodel
    /// </summary>
    [Serializable]
    public class AddSurveyedSurfaceProcessor : ICacheEntryProcessor<string, byte[], SurveyedSurface, bool>
    {
        public bool Process(IMutableCacheEntry<string, byte[]> entry, SurveyedSurface arg)
        {
            try
            {
                SurveyedSurfaces SurveyedSurfaces = entry.Exists ? SurveyedSurfaces.FromBytes(entry.Value) : new SurveyedSurfaces();

                SurveyedSurfaces.AddSurveyedSurfaceDetails(arg.ID, arg.DesignDescriptor, arg.AsAtDate);

                entry.Value = SurveyedSurfaces.ToByteArray();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

