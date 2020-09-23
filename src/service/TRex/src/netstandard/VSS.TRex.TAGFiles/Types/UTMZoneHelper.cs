namespace VSS.TRex.TAGFiles.Types
{
  public static class UTMZoneHelper
  {
    private const uint x3F = 0x3F;
    private const uint x1F = 0x1;

    /// <summary>
    /// Return the zone details for a given utmZone id, assuming zone group 'World wide/UTM' and datum 'WGS 1984'.
    /// </summary>
    public static (string zoneGroup, string zoneName) GetZoneDetailsFromUTMZone(int utmZone) =>
      (GetZoneGroup(utmZone), GetZone(utmZone));

    /// <summary>
    /// All TRex projections are calculated against transformations applied to the same World wide/UTM datum group. 
    /// </summary>
    private static string GetZoneGroup(int utmZone) => utmZone switch
    {
      61 => "Polar Regions/UPS",
      _ => "85,World wide/UTM",
    };

    /// <summary>
    /// Determines whether we're dealing with a polar region and what hemisphere the zone is within.
    /// </summary>
    private static string GetZone(int utmZone)
    {
      // Tag files can return one of 126 zones for the World wide/UTM datum group with rougly half being North and South.
      var hemisphereStr = (utmZone >> 6 & x1F) == 0
        ? "North"
        : "South";

      var zoneNumber = utmZone & x3F;

      return zoneNumber == 61
        ? hemisphereStr + " Pole" // For completeness, but zone 61 doesn't exist in the 85,World wide/UTM datum group of zones.
        : zoneNumber.ToString().PadLeft(1, ' ') + " " + hemisphereStr;
    }
  }
}
