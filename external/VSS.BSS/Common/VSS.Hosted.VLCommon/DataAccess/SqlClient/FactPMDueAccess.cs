using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{
  public static class FactPMDueAccess
  {
    public static int UpdateAssets(List<long> assetIDs)
    {
      // access to PM features should be restricted much earlier to here e.g. in the UI.
      //    Assets need to have valid, and current, service plans: 
      //    (int)ServiceTypeEnum.CATMAINT, (int)ServiceTypeEnum.VLMAINT, (int)ServiceTypeEnum.ManualMaintenanceLog };

      StoredProcDefinition sp = new StoredProcDefinition("NH_RPT", "uspPub_FactPMDue_Populate");
      sp.AddInputTable("@assetIDs", SqlAccessMethods.Fill(assetIDs, "AssetID"), "tbl_AssetList");
      return SqlAccessMethods.ExecuteNonQuery(sp);
    }
  }
}
