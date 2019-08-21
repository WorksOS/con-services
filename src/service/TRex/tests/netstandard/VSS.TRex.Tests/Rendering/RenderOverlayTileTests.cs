using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using System.Drawing;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Tests.Rendering
{
  public class RenderOverlayTileTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact()]
    public void Test_RenderOverlayTile_Creation()
    {
      var render = new RenderOverlayTile(Guid.NewGuid(),
        DisplayMode.Height,
        new XYZ(0, 0),
        new XYZ(100, 100),
        true, // CoordsAreGrid
        100, //PixelsX
        100, // PixelsY
        null, // Filters
        new DesignOffset(), // DesignDescriptor.Null(),
        null,
        Color.Black,
        string.Empty,
        new LiftParameters());

      render.Should().NotBeNull();
    }
  }
}
