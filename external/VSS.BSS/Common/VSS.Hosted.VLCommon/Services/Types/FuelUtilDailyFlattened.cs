using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{
  public class FuelUtilDailyFlattened : AssetUtilDailyFlattened
  {
    public double? TotalFuelConsumedGallons { get; set; }
    public double? WorkingFuelConsumedGallons { get; set; }
    public double? IdleFuelConsumedGallons { get; set; }
    public double? AverageBurnRateGallonsPerHour { get; set; }
    public int TotalFuelCalloutTypeID { get; set; }
    public int IdleFuelCalloutTypeID { get; set; }
  };


  public class FuelUtilDailyFlattenedComparer : IEqualityComparer<FuelUtilDailyFlattened>
  {
    public bool Equals(FuelUtilDailyFlattened x, FuelUtilDailyFlattened y)
    {
      return x.AssetID == y.AssetID;
    }

    public int GetHashCode(FuelUtilDailyFlattened obj)
    {
      return obj.AssetID.GetHashCode();
    }
  }
}

