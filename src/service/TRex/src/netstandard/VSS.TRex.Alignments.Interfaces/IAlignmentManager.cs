using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignmentManager
  {
    /// <summary>
    /// Add a new Alignment to a sitemodel
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    IAlignment Add(Guid siteModelUid, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents);

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    IAlignments List(Guid siteModelUid);

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    /// <param name="siteModelUid"></param>
    /// <param name="alignmentUid"></param>
    /// <returns></returns>
    bool Remove(Guid siteModelUid, Guid alignmentUid);
  }
}
