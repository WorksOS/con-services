using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.ConnectedSite.Gateway.Models;

using VSS.TRex.TAGFiles.Executors;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class ConnectedSiteMessageFactory
  {
    public static L1ConnectedSiteMessage BuildL1ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      return new L1ConnectedSiteMessage()
      {
        Timestamp = tagFilePrescan.SeedTimeUTC,
        Latitude = ConvertLLValue(tagFilePrescan.SeedLatitude),
        Longitude = ConvertLLValue(tagFilePrescan.SeedLongitude),
        Height = tagFilePrescan.SeedHeight,
        HardwareID = tagFilePrescan.HardwareID,
        PlatformType = tagFilePrescan.PlatformType,
      };
    }

    public static L2ConnectedSiteMessage BuildL2ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      return new L2ConnectedSiteMessage()
      {
        Timestamp = tagFilePrescan.SeedTimeUTC,
        Latitude = ConvertLLValue(tagFilePrescan.SeedLatitude),
        Longitude = ConvertLLValue(tagFilePrescan.SeedLongitude),
        Height = tagFilePrescan.SeedHeight,
        HardwareID = tagFilePrescan.HardwareID,
        AssetNickname = tagFilePrescan.MachineID,
        AppVersion = tagFilePrescan.ApplicationVersion,
        DesignName = tagFilePrescan.DesignName,
        AssetType = tagFilePrescan.MachineType.ToString(),
        PlatformType = tagFilePrescan.PlatformType
      };
    }

    private static double? ConvertLLValue(double? value)
    {
      if (value.HasValue && value.Value != Consts.NullDouble)
      {
        return MathUtilities.RadiansToDegrees(value.Value);
      }
      return null;
    }
  }


}
