using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.Services.Designs
{
    /// <summary>
    /// Interface detailing the API for the service that supports adding and managing designs
    /// </summary>
    public interface IDesignsService
    {
        /// <summary>
        /// Add a new surveyd surface to a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="AsAtDate"></param>
        void Add(long SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents);

        /// <summary>
        /// Request the list of surveyed surfaces from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        VSS.VisionLink.Raptor.Designs.Storage.Designs List(long SiteModelID);

        /// <summary>
        /// Removes a surveyed surfaces from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        bool Remove(long SiteModelID, long SurveySurfaceID);
    }
}
