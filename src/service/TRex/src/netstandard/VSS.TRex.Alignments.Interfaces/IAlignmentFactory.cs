using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignmentFactory
  {
    IAlignment NewInstance(Guid uId,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents);
  }
}
