using FluentAssertions;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class UTMZoneHelperTests
  {
    [Theory]
    [InlineData(62, "85,World wide/UTM", "62 North")]
    [InlineData(120, "85,World wide/UTM", "56 South")]
    [InlineData(56, "85,World wide/UTM", "56 North")]
    public void Should_return_correct_details_for_given_id(int utmZone, string expectedZoneGroup, string expectedZoneName)
    {
      var zone = UTMZoneHelper.GetZoneDetailsFromUTMZone(utmZone);

      zone.zoneGroup.Should().Be(expectedZoneGroup);
      zone.zoneName.Should().Be(expectedZoneName);
    }
  }
}
