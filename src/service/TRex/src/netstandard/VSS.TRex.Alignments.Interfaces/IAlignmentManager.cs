using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignmentManager
  {
    /// <summary>
    /// Add a new Alignment to a site model
    /// </summary>
    IAlignment Add(Guid siteModelUid, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents);

    /// <summary>
    /// List the surveyed surfaces for a site model
    /// </summary>
    IAlignments List(Guid siteModelUid);

    /// <summary>
    /// Remove a given surveyed surface from a site model
    /// </summary>
    bool Remove(Guid siteModelUid, Guid alignmentUid);

    /// <summary>
    /// Remove the list of alignments from the site model persisted storage
    /// </summary>
    public bool Remove(Guid siteModelId, IStorageProxy storageProxy);
  }
}
