using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// Sptial grid cache provides logic to determine which of the spatial data grid caches an application should read data from
    /// depending on it settings in RaptorServerConfig
    /// </summary>
    public static class SpatialGridCache
    {
        public const string kSpatialMutable = "Spatial-Mutable";
        public const string kSpatialImmutable = "Spatial-Immutable";
        public const string kSpatialImmutableCompressed = "Spatial-Immutable-Compressed";

        /// <summary>
        /// Returns the name of the spatial grid cache to use to locate cell and cell pass information
        /// </summary>
        public static String ReadCacheName()
        {
            if (RaptorServerConfig.Instance().UseMutableCellPassSegments)
            {
                return kSpatialMutable;
            }

            if (RaptorServerConfig.Instance().CompressImmutableCellPassSegments)
            {
                return kSpatialImmutableCompressed;
            }

            return kSpatialImmutable;
        }

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store mutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string WriteMutableCacheName()
        {
            return kSpatialMutable;
        }

        /// <summary>
        /// Returns the name of the spatial grid cache to use to store immutable cell and cell pass information
        /// </summary>
        /// <returns></returns>
        public static string WriteImmutableCacheName()
        {
            if (RaptorServerConfig.Instance().CompressImmutableCellPassSegments)
            {
                return kSpatialImmutableCompressed;
            }

            return kSpatialImmutable;
        }
    }
}
