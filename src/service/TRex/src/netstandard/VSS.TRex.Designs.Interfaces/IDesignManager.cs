using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignManager
  {
    /// <summary>
    /// Add a new design to a site model
    /// </summary>
    IDesign Add(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, ISubGridTreeBitMask existenceMap);

    IDesigns List(Guid SiteModelID);

    /// <summary>
    /// Remove a given design from a site model
    /// </summary>
    bool Remove(Guid siteModelID, Guid designID);

    /// <summary>
    /// Remove the design list for a site model from the persistent store
    /// </summary>
    bool Remove(Guid siteModelID, IStorageProxy storageProxy);
  }
}
