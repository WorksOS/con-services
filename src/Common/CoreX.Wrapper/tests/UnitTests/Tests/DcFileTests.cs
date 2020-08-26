using System;
using System.ComponentModel;
using CoreX.Interfaces;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  [Category("Slow")]
  public class DcFileTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _convertCoordinates;
    private const double LL_CM_TOLERANCE = 0.00000001;
    private const double GRID_CM_TOLERANCE = 0.01;

    public DcFileTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.CoreXWrapper;
    }

    public string GetCSIBFromDC(string dcFilename) => _convertCoordinates.DCFileToCSIB(DCFile.GetFilePath(dcFilename));

    [Fact]
    [Description("Tests that CoreX aborts when the geodetic data file defined in the CS geoid field cannot be located.")]
    public void Should_throw_when_geodetic_datafile_not_found()
    {
      var exObj = Record.Exception(() => GetCSIBFromDC(DCFile.NN2000_NORWAY18A));

      exObj.Should().NotBeNull("Expected InvalidOperationException when geodetic data file isn't found during CSIB extraction.");
      exObj.GetType().Should().Be<InvalidOperationException>();
      exObj.Message.Should().Be("GetCSIBFromDCFileContent: Geodata file not found for geoid model 'NN2000 (Norway18A)'");
    }

    [Theory]
    [InlineData("Húsavík.dc")]
    [InlineData("水島.dc")]
    [InlineData("鵜川ダム2018年原石山.dc")]
    public void Should_load_CS_files_with_multibyte_character_filenames(string dcFilename) => GetCSIBFromDC(dcFilename).Should().NotBeNullOrEmpty();

    [Theory]
    [Description("Sanity tests validating only height varies when VERT_ADJUST is present.")]
    [InlineData(36.207314496567186, -115.02494438822288, 608.9999852774359, ReturnAs.Degrees, DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST)]
    [InlineData(0.6319368512701705, -2.007564001497864, 608.99998527743593, ReturnAs.Radians, DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST)]
    [InlineData(36.207314496567186, -115.02494438822288, 550.9667886919241, ReturnAs.Degrees, DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST)]
    [InlineData(0.6319368512701705, -2.007564001497864, 550.96678869192413, ReturnAs.Radians, DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST)]
    public void Should_see_Height_values_differ_when_comparing_CS_files_with_VERT_ADJUST(double lat, double lon, double height, ReturnAs returnAs, string dcFilename)
    {
      var csib = GetCSIBFromDC(dcFilename);

      var xyz = _convertCoordinates.NEEToLLH(csib, new XYZ(2313, 1204, 609), returnAs);

      xyz.Should().NotBeNull();
      xyz.X.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      xyz.Y.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      xyz.Z.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    [Theory]
    [InlineData(52.2132598, 5.27894, 0, 469468.38343383482, 147600.70669055654, -43.151662645574675, InputAs.Degrees, DCFile.NETHERLANDS_DE_MIN)]
    [InlineData(0.911293297, 0.092134884, 0, 469468.38343383482, 147600.70669055654, -43.151662645574675, InputAs.Radians, DCFile.NETHERLANDS_DE_MIN)]
    [InlineData(0.8596496002217967, 0.14732153048180185, 0, 5457618.2482351921, 3459373.8527301643, -50.623720502480865, InputAs.Radians, DCFile.PHILIPSBURG)]
    [InlineData(-15.202778, 130.309167, 0, 8318824.308952088, 640622.3420586593, -39.303228628288906, InputAs.Degrees, "JRAC Bradshaw Jun07.cal")]
    public void Should_use_geodata_file_when_CS_defines_geoid(double lat, double lon, double height, double northing, double easting, double elevation, InputAs inputAs, string dcFilename)
    {
      var csib = GetCSIBFromDC(dcFilename);

      var nee = _convertCoordinates.LLHToNEE(csib, new LLH { Latitude = lat, Longitude = lon, Height = height }, inputAs);

      nee.Should().NotBeNull();
      nee.North.Should().BeApproximately(northing, GRID_CM_TOLERANCE);
      nee.East.Should().BeApproximately(easting, GRID_CM_TOLERANCE);
      nee.Elevation.Should().BeApproximately(elevation, GRID_CM_TOLERANCE);
    }

    [Theory(Skip = "Windows only")]
    [InlineData("CTCTSITECAL.dc", CSIB.CTCT_TEST_SITE)]
    public void Should_load_CS_file_and_return_CSIB(string dcFilename, string expectedCSIB)
    {
      var csib = GetCSIBFromDC(dcFilename);

      csib.Should().Be(expectedCSIB);
    }
  }
}
