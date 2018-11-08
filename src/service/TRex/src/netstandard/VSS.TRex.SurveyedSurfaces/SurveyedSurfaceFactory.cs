using System;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces
{
  public class SurveyedSurfaceFactory : ISurveyedSurfaceFactory
  {
    public ISurveyedSurface NewInstance(Guid iD,
      DesignDescriptor designDescriptor,
      DateTime asAtDate,
      BoundingWorldExtent3D extents)
    {
      return new SurveyedSurface(iD, designDescriptor, asAtDate, extents);
    }
  }
}
