using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.Common.Utilities;
using VSS.TRex.ConnectedSite.Gateway.Models;

using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Types;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class ConnectedSiteMessageFactory
  {
    public static L1ConnectedSiteMessage BuildL1ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      return new L1ConnectedSiteMessage()
      {
        Timestamp = tagFilePrescan.SeedTimeUTC,
        Lattitude = tagFilePrescan.SeedLatitude.HasValue ? (double?) MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLatitude.Value) : null,
        Longitude = tagFilePrescan.SeedLatitude.HasValue ? (double?) MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLongitude.Value) : null,
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
        Lattitude = tagFilePrescan.SeedLatitude.HasValue ? (double?) MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLatitude.Value) : null,
        Longitude = tagFilePrescan.SeedLatitude.HasValue ? (double?) MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLongitude.Value) : null,
        Height = tagFilePrescan.SeedHeight,
        HardwareID = tagFilePrescan.HardwareID,
        AssetNickname = tagFilePrescan.MachineID,
        AppVersion = tagFilePrescan.ApplicationVersion,
        DesignName = tagFilePrescan.DesignName,
        AssetType = ((MachineType) tagFilePrescan.MachineType).ToString(),
        PlatformType = tagFilePrescan.PlatformType
      };
    }
  }
}
