using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using VSS.Productivity3D.Models.Models.Files;
using VSS.TRex.Files.DXF;
using VSS.TRex.Geometry;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.TRex.Tests.Files
{
  public class DXFReaderTests
  {
    private Stream CreateTestPolyline()
    {
      byte[] bytes = null;

      using (var ms = new MemoryStream())
      {
        using (var writer = new StreamWriter(ms))
        {
          writer.WriteLine("0");
          writer.WriteLine("SECTION");
          writer.WriteLine("2");
          writer.WriteLine("HEADER");
          writer.WriteLine("9");
          writer.WriteLine("  $MEASUREMENT");
          writer.WriteLine("70");
          writer.WriteLine("0");
          writer.WriteLine("0");
          writer.WriteLine("ENDSEC");
          writer.WriteLine("0");
          writer.WriteLine("SECTION");
          writer.WriteLine("2");
          writer.WriteLine("ENTITIES");
          writer.WriteLine("0");
          writer.WriteLine("LWPOLYLINE");
          writer.WriteLine("90");
          writer.WriteLine("3"); // 3 vertces
          writer.WriteLine("70");
          writer.WriteLine("1"); // Polyline is closed
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("20");
          writer.WriteLine("0");
          writer.WriteLine("10");
          writer.WriteLine("10");
          writer.WriteLine("20");
          writer.WriteLine("0");
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("20");
          writer.WriteLine("10");
          writer.WriteLine("0");
          writer.WriteLine("ENDSEC");
          writer.WriteLine("0");
          writer.WriteLine("EOF");
        }

        bytes = ms.ToArray();
      }
      return new MemoryStream(bytes);
    }

    [Fact]
    public void Creation()
    {
      var reader = new DXFReader(null, DxfUnitsType.Meters);
      reader.Should().NotBeNull();
    }

    [Fact]
    public void ReadSimplePolyLine()
    {
      var reader = new DXFReader(CreateTestPolyline(), DxfUnitsType.Meters);
      reader.Should().NotBeNull();

      reader.FindEntitiesSection().Should().BeTrue();

      var success = reader.GetBoundaryFromPolyLineEntity(true, out var eof, out var boundary);
      success.Should().BeTrue();

      boundary.Should().NotBeNull();
      boundary.Name.Should().BeEmpty();
      boundary.Type.Should().Be(DXFLineWorkBoundaryType.Unknown);

      boundary.Boundary.Points.Count.Should().Be(3);
      boundary.Boundary.Points.Should().BeEquivalentTo(new List<FencePoint> {new FencePoint(0, 0), new FencePoint(10, 0), new FencePoint(0, 10)});
    }

    [Theory]
    [InlineData(DxfUnitsType.Meters, DistanceUnitsType.Meters)]
    [InlineData(DxfUnitsType.ImperialFeet, DistanceUnitsType.Feet)]
    [InlineData(DxfUnitsType.UsSurveyFeet, DistanceUnitsType.US_feet)]
    public void Units(DxfUnitsType sourceUnits, DistanceUnitsType destUnits)
    {
      var reader = new DXFReader(CreateTestPolyline(), sourceUnits);
      reader.Should().NotBeNull();

      reader.FindEntitiesSection().Should().BeTrue();

      var success = reader.GetBoundaryFromPolyLineEntity(true, out var eof, out var boundary);
      success.Should().BeTrue();

      boundary.Boundary.Points.Should().BeEquivalentTo(new List<FencePoint>
      {
        new FencePoint(0, 0), 
        new FencePoint(10 * UnitUtils.DistToMeters(destUnits), 0), 
        new FencePoint(0, 10 * UnitUtils.DistToMeters(destUnits))
      });
    }
  }
}
