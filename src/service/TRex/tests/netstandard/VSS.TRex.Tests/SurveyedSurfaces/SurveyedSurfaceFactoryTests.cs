using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces
{
  public class SurveyedSurfaceFactoryTests
  {
    [Fact]
    public void Creation()
    {
      var factory = new SurveyedSurfaceFactory();
      factory.Should().NotBeNull();
    }

    [Fact]
    public void NewInstance()
    {
      var factory = new SurveyedSurfaceFactory();
      var instance = factory.NewInstance(Guid.NewGuid(), DesignDescriptor.Null(), DateTime.MinValue, BoundingWorldExtent3D.Null());

      instance.Should().NotBeNull();
      instance.Should().BeOfType<ISurveyedSurface>();
    }
  }
}
