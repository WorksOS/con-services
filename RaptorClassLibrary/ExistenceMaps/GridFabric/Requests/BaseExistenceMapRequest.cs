using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Base class for existence maps requests. Defines existnace map type descriptors and related base functionality sucj as cachey key calculation
    /// </summary>
    public class BaseExistenceMapRequest
    {
        protected const string EXISTENCE_MAP_HEADER = "SpatialExistenceMap";
        protected const int EXISTENCE_MAP_VERSION = 1;

        public const int EXISTANCE_MAP_DESIGN_DESCRIPTOR = 1;
        public const int EXISTANCE_SURVEYED_SURFACE_DESCRIPTOR = 2;

        /// <summary>
        /// Constrct a unique key for the existance map comprised of a type descriptor and an ID
        /// </summary>
        /// <param name="typeDescriptor"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public string CacheKey(long siteModelID, long typeDescriptor, long ID) => $"Model:{siteModelID}-Descriptor:{typeDescriptor}=ID:{ID}";
    }
}
