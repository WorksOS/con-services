using System;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public static class ProjectSiteMonitoringAccess
  {
	  public class ProjectRouteSummary
    {
      public long srcSiteID = -1;
      public string srcSiteName = null;
      public long dstSiteID = -1;
      public string dstSiteName = null;

      public double? targetVolumeCubicMeter = double.NaN;
      public bool manyToMany = false;
      public double? actualVolumeCubicMeter = null;

      public double? targetCostPerCubicMeterPerKm = double.NaN;
      public double? actualCostPerCubicMeterPerKm = double.NaN;

      //Note the values returned by the sproc for these 2 are probably incorrect for many-many relationship.
      //TODO: Need to check this.
      public double? totalTargetCost = double.NaN;
      public double? totalActualCost = double.NaN;

      public int massHaulTypeID;

      //TODO: Need to sync with DB layer
      public DateTime? lastUpdateTime;
    }
 
	}
}
