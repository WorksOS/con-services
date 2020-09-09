using System.IO;
using System.Runtime.InteropServices;
using CoreX.Interfaces;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoordinateSystemTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _coreX;
    private string GetFileContent(string dcFilename) => File.ReadAllText(DCFile.GetFilePath(dcFilename));

    public CoordinateSystemTests(UnitTestBaseFixture testFixture)
    {
      _coreX = testFixture.CoreXWrapper;
    }

    [Fact]
    public void Should_return_datum_list()
    {
      var datums = _coreX.GetDatums();

      datums.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("2597,Tonga/GD2005", "Tonga Map Grid", CSIB.TONGA_2598_MAP_GRID)]
    [InlineData("85,World wide/UTM", "10 North", CSIB.WORLD_WIDE_UTM_GROUP_ZONE_10_NORTH)]
    [InlineData("85,World wide/UTM", "13 North", CSIB.WORLD_WIDE_UTM_GROUP_ZONE_13_NORTH)]
    public void Should_return_CSIB_for_given_zoneGroup_and_zoneName_with_no_datum(string zoneGroupName, string zoneName, string expectedCsib)
    {
      var csib = _coreX.GetCSIBFromCSDSelection(zoneGroupName, zoneName);

      csib.Should().NotBeNull();

      // Base64 string encoding varies between OS, what works on Windows doesn't work in CI env.
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        csib.Should().Be(expectedCsib);
      }
    }

    [Theory]
    [InlineData(null, "DC file string cannot be null (Parameter 'dcFileStr')")]
    [InlineData(DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST, "Error 'cecDCParseLineNotEndingWithLF' attempting to retrieve the DC file's CSD")]
    public void Should_throw_for_badly_formatted_file_string(string dcFileString, string expectedErrorMessage)
    {
      var ex = Record.Exception(() => _coreX.GetCSDFromDCFileContent(dcFileString));

      ex.Message.Should().Be(expectedErrorMessage);
    }

    [Theory]
    [InlineData(DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST)]
    public void Should_return_CoordinateSystem_for_valid_DC_file_string(string dcFileString)
    {
      var result = _coreX.GetCSDFromDCFileContent(GetFileContent(dcFileString));

      result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(CSIB.DIMENSIONS_2012_WITHOUT_VERT_ADJUST)]
    public void Should_return_CoordinateSystem_for_valid_CSIB_string(string csib)
    {
      var result = _coreX.GetCSDFromCSIB(csib);

      result.Should().NotBeNull();
    }
  }
}
