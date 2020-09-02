using FluentAssertions;
using VSS.TRex.TAGFiles.Types;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class UTMZoneHelperTests
  {
    /// <summary>
    /// utmZones are ordered in CoreX with the first 63 == North, and those > 63 == South. So the utmZone for 'adjacent' zones may have 
    /// wildly different zone id values in the tag file, they resolve to adjacent zoneIds from CoreX, e.g. 120 and 56 are '935,55 South'
    /// and '936,56 North' respectively. E.g. see CsdManagement.csmGetDatumFromCSDSelectionById((uint)datumSystemId, false, null, null, datumContainer);
    /// </summary>
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
