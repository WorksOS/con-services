using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Alignments.Interfaces;

namespace VSS.TRex.Alignments
{
  public class AlignmentFactory : IAlignmentFactory
  {
    public IAlignment NewInstance(Guid uId,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents)
    {
      return new Alignment(uId, designDescriptor, extents);
    }
  }
}
