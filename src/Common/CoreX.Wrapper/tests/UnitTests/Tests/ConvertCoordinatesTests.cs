using System.Linq;
using CoreX.Models;
using CoreX.Wrapper.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class ConvertCoordinatesTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly IConvertCoordinates _convertCoordinates;
    private readonly string _csib;

    private const double LL_CM_TOLERANCE = 0.00000001;
    private const double GRID_CM_TOLERANCE = 0.01;

    public ConvertCoordinatesTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.ConvertCoordinates;

      testFixture.CoreX.SetCSIBFromDCFile(DCFile.GetFilePath(DCFile.BOOTCAMP_2012));
      _csib = testFixture.CoreX.CSIB;
    }

    [Theory]
    [InlineData(36.21730699569774, -115.0372771786517, 550.8719470044193, ReturnAs.Degrees)]
    [InlineData(0.63211125328050133, -2.007779249296807, 550.87194700441933, ReturnAs.Radians)]
    public void CoordinateService_SimpleXYZNEEToLLH(double lat, double lon, double height, ReturnAs returnAs)
    {
      var xyz = _convertCoordinates.NEEToLLH(_csib, new XYZ(2313, 1204, 609), returnAs);

      xyz.Should().NotBeNull();
      xyz.X.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      xyz.Y.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      xyz.Z.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(36.21730699569774, -115.0372771786517, 550.8719470044193, ReturnAs.Degrees)]
    [InlineData(0.63211125328050133, -2.007779249296807, 550.87194700441933, ReturnAs.Radians)]
    public void CoordinateService_SimpleNEEToLLH(double lat, double lon, double height, ReturnAs returnAs)
    {
      var llhCoords = _convertCoordinates.NEEToLLH(_csib,
        new NEE
        {
          North = 1204,
          East = 2313,
          Elevation = 609
        }, returnAs);

      llhCoords.Should().NotBeNull();
      llhCoords.Latitude.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      llhCoords.Longitude.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      llhCoords.Height.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(1204.00000000004, 2312.999999989388, 609.0)]
    public void CoordinateService_SimpleLLHToNEE(double toNorthing, double toEasting, double toElevation)
    {
      var neeCoords = _convertCoordinates.LLHToNEE(_csib,
        new LLH
        {
          Latitude = 0.63211125328050133,
          Longitude = -2.007779249296807,
          Height = 550.87194700441933
        }, InputAs.Radians);

      neeCoords.Should().NotBeNull();
      neeCoords.North.Should().BeApproximately(toNorthing, GRID_CM_TOLERANCE);
      neeCoords.East.Should().BeApproximately(toEasting, GRID_CM_TOLERANCE);
      neeCoords.Elevation.Should().BeApproximately(toElevation, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ManyLLHToNEE()
    {
      var neeCoords = _convertCoordinates.LLHToNEE(_csib,
        new[]
        {
          new LLH { Latitude = 0.63211125328050133, Longitude = -2.007779249296807, Height = 550.87194700441933 },
          new LLH { Latitude = 0.63211125328050133, Longitude = -2.007779249296807, Height = 550.87194700441933 }
        }, InputAs.Radians).ToList();

      neeCoords.Should().NotBeNull();

      neeCoords[0].North.Should().BeApproximately(1204.00000000004, GRID_CM_TOLERANCE);
      neeCoords[0].East.Should().BeApproximately(2312.999999989388, GRID_CM_TOLERANCE);
      neeCoords[0].Elevation.Should().BeApproximately(609.0, GRID_CM_TOLERANCE);

      neeCoords[1].North.Should().BeApproximately(1204.00000000004, GRID_CM_TOLERANCE);
      neeCoords[1].East.Should().BeApproximately(2312.999999989388, GRID_CM_TOLERANCE);
      neeCoords[1].Elevation.Should().BeApproximately(609.0, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ManyNEEToLLH()
    {
      var requestArray = new[] {
        new NEE { East = 2313, North = 1204, Elevation = 609 },
        new NEE { East = 2313, North = 1204, Elevation = 609 }
      };

      var llhCoords = _convertCoordinates.NEEToLLH(_csib, requestArray, ReturnAs.Degrees);

      llhCoords.Should().NotBeNull();

      llhCoords[0].Longitude.Should().BeApproximately(-115.03727717865179, LL_CM_TOLERANCE);
      llhCoords[0].Latitude.Should().BeApproximately(36.21730699569774, LL_CM_TOLERANCE);
      llhCoords[0].Height.Should().BeApproximately(550.87194700441933, LL_CM_TOLERANCE);

      llhCoords[1].Longitude.Should().BeApproximately(-115.03727717865179, LL_CM_TOLERANCE);
      llhCoords[1].Latitude.Should().BeApproximately(36.21730699569774, LL_CM_TOLERANCE);
      llhCoords[1].Height.Should().BeApproximately(550.87194700441933, LL_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_SimpleXYZLLHToNEE()
    {
      var xyz = _convertCoordinates.LLHToNEE(_csib, new XYZ(-115.01, 36.21, 10), InputAs.Degrees);

      xyz.Should().NotBeNull();
      xyz.Y.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
      xyz.X.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      xyz.Z.Should().BeApproximately(68.058950967814724, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ManyXYZLLHToNEE()
    {
      var coords = new[]
      {
        new XYZ(-115.01, 36.21, 10),
        new XYZ(-115.02, 36.22, 11),
        new XYZ(-115.03, 36.23, 12)
      };

      var neeCoords = _convertCoordinates.LLHToNEE(_csib, coords, InputAs.Degrees);

      neeCoords.Should().NotBeNull();

      neeCoords[0].Y.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
      neeCoords[0].X.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      neeCoords[0].Z.Should().BeApproximately(68.058950967814724, GRID_CM_TOLERANCE);

      neeCoords[1].Y.Should().BeApproximately(2757.6347846893877, GRID_CM_TOLERANCE);
      neeCoords[1].X.Should().BeApproximately(2611.7640792344355, GRID_CM_TOLERANCE);
      neeCoords[1].Z.Should().BeApproximately(69.1538811614891, GRID_CM_TOLERANCE);

      neeCoords[2].Y.Should().BeApproximately(1858.4988322410918, GRID_CM_TOLERANCE);
      neeCoords[2].X.Should().BeApproximately(3721.5247073087949, GRID_CM_TOLERANCE);
      neeCoords[2].Z.Should().BeApproximately(70.248819491614839, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ManyXYZNEEToLLH()
    {
      var requestArray = new[] {
        new XYZ(2313, 1204, 609),
        new XYZ(2314, 1205, 610)
      };

      var llhCoords = _convertCoordinates.NEEToLLH(_csib, requestArray, ReturnAs.Degrees);

      llhCoords.Should().NotBeNull();

      llhCoords[0].X.Should().BeApproximately(-115.03727717865179, LL_CM_TOLERANCE);
      llhCoords[0].Y.Should().BeApproximately(36.21730699569774, LL_CM_TOLERANCE);
      llhCoords[0].Z.Should().BeApproximately(550.87194700441933, LL_CM_TOLERANCE);

      llhCoords[1].X.Should().BeApproximately(-115.03726605986158, LL_CM_TOLERANCE);
      llhCoords[1].Y.Should().BeApproximately(36.217316008131625, LL_CM_TOLERANCE);
      llhCoords[1].Z.Should().BeApproximately(551.87186118441252, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(3656.9996220201547, 1502.0980247307239, 68.058950967814724)]
    public void CoordinateService_SimpleWGS84PointToXYZNEE(double easting, double northing, double elevation)
    {
      var neeCoords = _convertCoordinates.WGS84ToCalibration(_csib, new WGS84Point(-115.01, 36.21, 10));

      neeCoords.Should().NotBeNull();
      neeCoords.Y.Should().BeApproximately(easting, GRID_CM_TOLERANCE);
      neeCoords.X.Should().BeApproximately(northing, GRID_CM_TOLERANCE);
      neeCoords.Z.Should().BeApproximately(elevation, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ManyWGS84PointToXYZNEE()
    {
      var points = new[]
      {
        new WGS84Point(-115.01, 36.21, 10),
        new WGS84Point(-115.02, 36.22, 11)
      };

      var neeCoords = _convertCoordinates.WGS84ToCalibration(_csib, points);

      neeCoords.Should().NotBeNull();

      neeCoords[0].Y.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
      neeCoords[0].X.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      neeCoords[0].Z.Should().BeApproximately(68.058950967814724, GRID_CM_TOLERANCE);

      neeCoords[1].Y.Should().BeApproximately(2757.6347846893877, GRID_CM_TOLERANCE);
      neeCoords[1].X.Should().BeApproximately(2611.7640792344355, GRID_CM_TOLERANCE);
      neeCoords[1].Z.Should().BeApproximately(69.1538811614891, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_ImportCSIBFromDC()
    {
      var csib = _convertCoordinates.DCFileToCSIB(DCFile.GetFilePath(DCFile.BOOTCAMP_2012));

      csib.Should().Be(_csib);
    }
  }
}
