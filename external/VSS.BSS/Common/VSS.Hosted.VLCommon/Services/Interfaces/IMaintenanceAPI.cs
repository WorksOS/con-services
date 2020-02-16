using System;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{
  public interface IMaintenanceAPI
  {
    long SaveAssetInterval(INH_OP ctx, long assetID, PMInterval newInterval, PMTrackingTypeEnum trackingType, bool callPMAPIForAllMCIntervals = true);   
    bool SetAssetIntervalDynamicUpdate(INH_OP ctx, long assetID, bool isDynamicUpdate);
    PMIntervalWithNextDueInfo GetNextDueInterval(INH_OP opCtx, long assetID, double currentRuntimeHours, double currentOdometerMiles, List<PMIntervalWithNextDueInfo> allIntervals);
    PMIntervalWithNextDueInfo GetGreatestCumulativeIntervalWithinXPercentOfBeingDue(INH_OP ctx, long assetID, List<PMIntervalWithNextDueInfo> allIntervalsWithNextDueHours, PMInterval overdueCumulativeInterval, double currentRuntimeHours, int thresholdPercent);
    double? GetExpectedWeeklyMileage(INH_OP ctx, long assetID);
    bool CopyIntervalToAsset(INH_OP ctx, long sourceAssetID, long targetAssetID, bool copyChecklistAndParts, bool copyServiceLevelIntervals, bool copyIndependentIntervals, bool copyMajorComponentIntervals);
   
    List<string> GetParentAccounts(long customerID);
    bool SavePMModifiedInterval(long? userId, long assetID, long intervalId, int modifiedRuntimeHours);
    bool StartTrackingMaintenance(SessionContext session, long assetID, List<PMInterval> pmIntervals, double runtimeHours);
    List<int> GetSupportedDeviceTypesForManualMaintenance();
  }
}
