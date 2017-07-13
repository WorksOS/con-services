using System;

namespace TestUtility.Model.WebApi
{
  public class AssetCycleData
    {

        public string assetUid { get; set; }
        public string assetName { get; set; }
        public string serialNumber { get; set; }
        public int? assetIcon { get; set; }
        public string makeCode { get; set; }
        public string model { get; set; }
        public string lastReportedTime { get; set; }
        public int? cycleCount { get; set; }
        public double? distanceTravelledKm { get; set; }
        public double? volumeCubicMeter { get; set; }

        public override bool Equals(object obj)
        {
            var actual = obj as AssetCycleData;

            if (distanceTravelledKm != null)
            { 
                if (Math.Round((double)distanceTravelledKm, 2) != Math.Round((double)actual.distanceTravelledKm, 2))
                    return false;
            }

            if (volumeCubicMeter != null)
            { 
                if (Math.Round((double)volumeCubicMeter, 2) != Math.Round((double)actual.volumeCubicMeter, 2))
                    return false;
            }

            if (cycleCount != null)
            { 
                if (cycleCount != actual.cycleCount)
                    return false;
            }

            if (lastReportedTime != null)
            {
                               
                if (DateTimeOffset.Parse(lastReportedTime).DateTime  != DateTimeOffset.Parse(actual.lastReportedTime).DateTime)
                {
                    return false;
                }
            }

            if (assetIcon != null)
            {
                if (assetIcon != actual.assetIcon)
                {
                    return false;
                }
            }

            if (assetName != null)
            {
                if (assetName != actual.assetName)
                {
                    return false;
                }
            }

            if (serialNumber != null)
            {
                if (serialNumber != actual.serialNumber)
                {
                    return false;
                }
            }
            if (makeCode != null)
            {
                if (makeCode != actual.makeCode)
                {
                    return false;
                }
            }
            if (model != null)
            {
                if (model != actual.model)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
