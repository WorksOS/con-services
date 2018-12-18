using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignManager
  {
    /// <summary>
    /// Add a new design to a sitemodel
    /// </summary>
    /// <param name="SiteModelID"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    IDesign Add(Guid SiteModelID, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents);

    IDesigns List(Guid SiteModelID);

    /// <summary>
    /// Remove a given design from a site model
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="designID"></param>
    /// <returns></returns>
    bool Remove(Guid siteModelID, Guid designID);
  }
}
