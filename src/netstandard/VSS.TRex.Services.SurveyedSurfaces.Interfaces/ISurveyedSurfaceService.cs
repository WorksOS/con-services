using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Services.Surfaces
{
    /// <summary>
    /// Interface detailing the API for the service that supports adding and managing surveyed surfaces
    /// </summary>
    public interface ISurveyedSurfaceService
    {
        /// <summary>
        /// Add a new surveyd surface to a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="AsAtDate"></param>
        /// <param name="extents"></param>
        void Add(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime AsAtDate, BoundingWorldExtent3D extents);

        /// <summary>
        /// Request the list of surveyed surfaces from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        ISurveyedSurfaces List(Guid SiteModelID);

        /// <summary>
        /// Removes a surveyed surfaces from a site model
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <param name="SurveySurfaceID"></param>
        /// <returns></returns>
        bool Remove(Guid SiteModelID, Guid SurveySurfaceID);
    }
}
