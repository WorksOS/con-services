using System.Linq;
using CoreX.Interfaces;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class ConvertCoordinatesTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _convertCoordinates;

    private const double LL_CM_TOLERANCE = 0.00000001;
    private const double GRID_CM_TOLERANCE = 0.01;

    private readonly string _csib;

    public ConvertCoordinatesTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.CoreXWrapper;
      _csib = testFixture.CSIB;
    }

    public string GetCSIBFromDC(string dcFilename) => _convertCoordinates.DCFileToCSIB(DCFile.GetFilePath(dcFilename));

    [Theory]
    [InlineData(36.207314496567186, -115.02494438822288, 550.9667886919241, ReturnAs.Degrees)]
    [InlineData(0.6319368512701705, -2.007564001497864, 550.9667886919241, ReturnAs.Radians)]
    public void CoordinateService_SimpleXYZNEEToLLH(double lat, double lon, double height, ReturnAs returnAs)
    {
      var xyz = _convertCoordinates.NEEToLLH(_csib, new XYZ(2313, 1204, 609), returnAs);

      xyz.Should().NotBeNull();
      xyz.X.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      xyz.Y.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      xyz.Z.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(804954.336119, 388234.958576, 0, -0.7600188962232834, 3.0121130490077275, 11.395071419290979, ReturnAs.Radians, CSIB.CTCT_TEST_SITE)]
    public void CoordinateService_SimpleXYZToLLH(double northing, double easting, double elevation, double lat, double lon, double height, ReturnAs returnAs, string csib)
    {
      var llhCoords = _convertCoordinates.NEEToLLH(csib,
        new XYZ
        {
          X = easting,
          Y = northing,
          Z = elevation
        }, returnAs);

      llhCoords.Should().NotBeNull();
      llhCoords.Y.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      llhCoords.X.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      llhCoords.Z.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(36.207314496567186, -115.02494438822288, 550.9667886919241, ReturnAs.Degrees)]
    [InlineData(0.6319368512701705, -2.007564001497864, 550.9667886919241, ReturnAs.Radians)]
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
    [InlineData(0.63211125328050133, -2.007779249296807, 550.87194700441933, 2312.999999989388, 1204.00000000004, 609.0, InputAs.Radians, CSIB.DIMENSIONS_2012_WITH_VERT_ADJUST)]
    [InlineData(0.8596496002217967, 0.14732153048180185, 0, 5457618.2482351921, 3459373.8527301643, -50.623720502480865, InputAs.Radians, CSIB.PHILIPSBURG)]
    public void CoordinateService_SimpleLLHToNEE(double lat, double lon, double height, double northing, double easting, double elevation, InputAs inputAs, string csib)
    {
      var neeCoords = _convertCoordinates.LLHToNEE(csib,
        new LLH
        {
          Latitude = lat,
          Longitude = lon,
          Height = height
        }, inputAs);

      neeCoords.Should().NotBeNull();
      neeCoords.North.Should().BeApproximately(northing, GRID_CM_TOLERANCE);
      neeCoords.East.Should().BeApproximately(easting, GRID_CM_TOLERANCE);
      neeCoords.Elevation.Should().BeApproximately(elevation, GRID_CM_TOLERANCE);
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

      neeCoords[0].North.Should().BeApproximately(2312.999999989388, GRID_CM_TOLERANCE);
      neeCoords[0].East.Should().BeApproximately(1204.00000000004, GRID_CM_TOLERANCE);
      neeCoords[0].Elevation.Should().BeApproximately(609.0, GRID_CM_TOLERANCE);

      neeCoords[1].North.Should().BeApproximately(2312.999999989388, GRID_CM_TOLERANCE);
      neeCoords[1].East.Should().BeApproximately(1204.00000000004, GRID_CM_TOLERANCE);
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

      llhCoords[0].Longitude.Should().BeApproximately(-115.02494438822288, LL_CM_TOLERANCE);
      llhCoords[0].Latitude.Should().BeApproximately(36.207314496567186, LL_CM_TOLERANCE);
      llhCoords[0].Height.Should().BeApproximately(550.9667886919241, LL_CM_TOLERANCE);

      llhCoords[1].Longitude.Should().BeApproximately(-115.02494438822288, LL_CM_TOLERANCE);
      llhCoords[1].Latitude.Should().BeApproximately(36.207314496567186, LL_CM_TOLERANCE);
      llhCoords[1].Height.Should().BeApproximately(550.9667886919241, LL_CM_TOLERANCE);
    }

    [Fact]
    public void CoordinateService_SimpleXYZLLHToNEE()
    {
      var xyz = _convertCoordinates.LLHToNEE(_csib, new XYZ(-115.01, 36.21, 10), InputAs.Degrees);

      xyz.Should().NotBeNull();
      xyz.Y.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      xyz.X.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
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

      neeCoords[0].Y.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      neeCoords[0].X.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
      neeCoords[0].Z.Should().BeApproximately(68.058950967814724, GRID_CM_TOLERANCE);

      neeCoords[1].Y.Should().BeApproximately(2611.7640792344355, GRID_CM_TOLERANCE);
      neeCoords[1].X.Should().BeApproximately(2757.6347846893877, GRID_CM_TOLERANCE);
      neeCoords[1].Z.Should().BeApproximately(69.1538811614891, GRID_CM_TOLERANCE);

      neeCoords[2].Y.Should().BeApproximately(3721.5247073087949, GRID_CM_TOLERANCE);
      neeCoords[2].X.Should().BeApproximately(1858.4988322410918, GRID_CM_TOLERANCE);
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

      llhCoords[0].X.Should().BeApproximately(-115.02494438822288, LL_CM_TOLERANCE);
      llhCoords[0].Y.Should().BeApproximately(36.207314496567186, LL_CM_TOLERANCE);
      llhCoords[0].Z.Should().BeApproximately(550.9667886919241, LL_CM_TOLERANCE);

      llhCoords[1].X.Should().BeApproximately(-115.02493326943193, LL_CM_TOLERANCE);
      llhCoords[1].Y.Should().BeApproximately(36.207323507870385, LL_CM_TOLERANCE);
      llhCoords[1].Z.Should().BeApproximately(551.9667028719174, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(36.21, -115.01, 10, 1502.0980247307239, 3656.9996220201547, 68.058950967814724, InputAs.Degrees)]
    [InlineData(0.63211125328050133, -2.007779249296807, 550.87194700441933, 2313, 1204, 609, InputAs.Radians)]
    [InlineData(0.63211125328050133, -2.007779249296807, TestConsts.NULL_DOUBLE, 2313, 1204, TestConsts.NULL_DOUBLE, InputAs.Radians)]
    public void Should_convert_a_WGS84Point_to_XYZ_grid_coordinate(double lat, double lon, double height, double toY, double toX, double toZ, InputAs inputAs)
    {
      var xyzCoords = _convertCoordinates.WGS84ToCalibration(
        _csib,
        new WGS84Point(lon: lon, lat: lat, height: height),
        inputAs);

      xyzCoords.Should().NotBeNull();
      xyzCoords.Y.Should().BeApproximately(toY, GRID_CM_TOLERANCE);
      xyzCoords.X.Should().BeApproximately(toX, GRID_CM_TOLERANCE);
      xyzCoords.Z.Should().BeApproximately(toZ, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void Should_convert_many_WGS84Point_to_XYZ_grid_coordinates()
    {
      var points = new[]
      {
        new WGS84Point(lon:-115.01, lat: 36.21, height: 10),
        new WGS84Point(lon:-115.02, lat: 36.22, height: 11)
      };

      var xyzCoords = _convertCoordinates.WGS84ToCalibration(_csib, points, InputAs.Degrees);

      xyzCoords.Should().NotBeNull();

      xyzCoords[0].Y.Should().BeApproximately(1502.0980247307239, GRID_CM_TOLERANCE);
      xyzCoords[0].X.Should().BeApproximately(3656.9996220201547, GRID_CM_TOLERANCE);
      xyzCoords[0].Z.Should().BeApproximately(68.058950967814724, GRID_CM_TOLERANCE);

      xyzCoords[1].Y.Should().BeApproximately(2611.7640792344355, GRID_CM_TOLERANCE);
      xyzCoords[1].X.Should().BeApproximately(2757.6347846893877, GRID_CM_TOLERANCE);
      xyzCoords[1].Z.Should().BeApproximately(69.1538811614891, GRID_CM_TOLERANCE);
    }

    [Fact]
    public void WGS84Point_with_NULL_height_should_return_invalid_result()
    {
      var points = new WGS84Point(lon: 0.14729266728569143, lat: 0.8596927023775642, height: TestConsts.NULL_DOUBLE);

      var xyzCoords = _convertCoordinates.WGS84ToCalibration(CSIB.PHILIPSBURG, points, InputAs.Radians);

      xyzCoords.Should().NotBeNull();

      xyzCoords.Y.Should().Be(0);
      xyzCoords.X.Should().Be(0);
      xyzCoords.Z.Should().Be(0);
    }

    [Fact]
    public void WGS84Point_with_ZERO_height_should_return_valid_result()
    {
      var points = new WGS84Point(lon: 0.14729266728569143, lat: 0.8596927023775642, height: 0);

      var xyzCoords = _convertCoordinates.WGS84ToCalibration(CSIB.PHILIPSBURG, points, InputAs.Radians);

      xyzCoords.Should().NotBeNull();

      xyzCoords.Y.Should().BeApproximately(5457893.789346485, GRID_CM_TOLERANCE);
      xyzCoords.X.Should().BeApproximately(3459255.4981716694, GRID_CM_TOLERANCE);
      xyzCoords.Z.Should().BeApproximately(-50.61838511098176, GRID_CM_TOLERANCE);
    }


    [Fact]
    public void ManyWGS84Point_with_null_height_to_XYZNEE()
    {
      var points = new[]
      {
        new WGS84Point(lon: 0.14732153048180185, lat: 0.8596496002217967, height: 0)
      };

      var csib = GetCSIBFromDC(DCFile.PHILIPSBURG);
      var xyzCoords = _convertCoordinates.WGS84ToCalibration(csib, points, InputAs.Radians);

      xyzCoords.Should().NotBeNull();

      xyzCoords[0].Y.Should().BeApproximately(5457618.2482351921, GRID_CM_TOLERANCE);
      xyzCoords[0].X.Should().BeApproximately(3459373.8527301643, GRID_CM_TOLERANCE);
      xyzCoords[0].Z.Should().BeApproximately(-50.623720502480865, GRID_CM_TOLERANCE);
    }
  }
}
