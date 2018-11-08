using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurfaceManager
  {
    /// <summary>
    /// Add a new surveyed surface to a sitemodel
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    ISurveyedSurface Add(Guid SiteModelID, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents);

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    ISurveyedSurfaces List(Guid SiteModelID);

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="SurveySurfaceID"></param>
    /// <returns></returns>
    bool Remove(Guid SiteModelID, Guid SurveySurfaceID);
  }
}
