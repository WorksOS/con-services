using System;

namespace VSS.Hosted.VLCommon
{
  public  class AssetCurrentStatusInfo
    {
      public long AssetID;
      public DateTime? LastRuntimeHoursUTC = null;
      public double? RuntimeHours = null;
      public double? Mileage = null;
    }
}
