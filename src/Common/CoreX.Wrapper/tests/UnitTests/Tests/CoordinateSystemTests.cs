using CoreX.Interfaces;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoordinateSystemTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _coreX;

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
    [InlineData("2597,Tonga/GD2005", "2598,Tonga Map Grid", CSIB.TONGA_2598_MAP_GRID)]
    public void Should_return_CSIB_for_given_zoneGroup_and_zoneName(string zoneGroupName, string zoneName, string expectedCsib)
    {
      var csib = _coreX.GetCoordinateSystemFromCSDSelection(zoneGroupName, zoneName);

      csib.Should().Be(expectedCsib);
    }

    [Theory]
    [InlineData("85,World wide/UTM", "838,10 North", CSIB.WORLD_WIDE_UTM_GROUP_ZONE_10_NORTH)]
    public void Should_return_CSIB_for_given_zoneGroup_and_zoneName_with_no_datum(string zoneGroupName, string zoneName, string expectedCsib)
    {
      var csib = _coreX.GetCoordinateSystemFromCSDSelection(zoneGroupName, zoneName);

      csib.Should().Be(expectedCsib);
    }
  }
}
