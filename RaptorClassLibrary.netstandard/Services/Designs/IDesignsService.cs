using System;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.Services.Designs
{
    /// <summary>
    /// Interface detailing the API for the service that supports adding and managing designs
    /// </summary>
    public interface IDesignsService
    {
        /// <summary>
        /// Add a new design to a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="extents"></param>
        void Add(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents);

        /// <summary>
        /// Request the list of designs from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designID"></param>
        /// <returns></returns>
        Raptor.Designs.Storage.Design Find(Guid SiteModelID, long designID);

        /// <summary>
        /// Request the list of designs from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        Raptor.Designs.Storage.Designs List(Guid SiteModelID);

        /// <summary>
        /// Removes a design from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="DesignID"></param>
        /// <returns></returns>
        bool Remove(Guid SiteModelID, long DesignID);
    }
}
