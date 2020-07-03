using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurfaceManager
  {
    /// <summary>
    /// Add a new surveyed surface to a site model
    /// </summary>
    ISurveyedSurface Add(Guid siteModelUid, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents, ISubGridTreeBitMask existenceMap);

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    ISurveyedSurfaces List(Guid siteModelUid);

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    bool Remove(Guid siteModelUid, Guid surveySurfaceUid);

    /// <summary>
    /// Remove the surveyed surface list for a site model from the persistent store
    /// </summary>
    bool Remove(Guid siteModelUid, IStorageProxy storageProxy);
  }
}
