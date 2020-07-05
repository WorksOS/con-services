using System;
using System.Collections.Generic;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesigns : IList<IDesign>, IBinaryReaderWriter
  {
    /// <summary>
    /// Create a new design in the list based on the provided details
    /// </summary>
    IDesign AddDesignDetails(Guid ADesignID,
      DesignDescriptor ADesignDescriptor,
      BoundingWorldExtent3D AExtents);

    /// <summary>
    /// Remove a given design from the list of designs for a site model
    /// </summary>
    bool RemoveDesign(Guid ADesignID);

    IDesign Locate(Guid AID);
  }
}
