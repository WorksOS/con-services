using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurfaceManager
  {
    /// <summary>
    /// Add a new surveyed surface to a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    ISurveyedSurface Add(Guid siteModelUid, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents);

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    ISurveyedSurfaces List(Guid siteModelUid);

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="surveySurfaceUid"></param>
    /// <returns></returns>
    bool Remove(Guid siteModelUid, Guid surveySurfaceUid);
  }
}
