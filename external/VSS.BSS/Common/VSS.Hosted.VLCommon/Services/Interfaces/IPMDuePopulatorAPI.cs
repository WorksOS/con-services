using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public interface IPMDuePopulatorAPI
  {
    void PopulatePMDue(INH_OP ctx, long assetID, long? userID = null, int resetModifiedRuntimehours = 0, List<PMCompletedInstance> clearedInstances = null, bool isResetIntervalsToDefault = false);
    bool CallPMPopulatorForAssets(INH_OP ctx, List<PMSalesModel> salesModels);

    List<PMCompletedInstance> GetAllPMCompletedInstancesPerAsset(INH_OP ctx, long assetID, bool isResetIntervalsToDefault=false);
  }
}
