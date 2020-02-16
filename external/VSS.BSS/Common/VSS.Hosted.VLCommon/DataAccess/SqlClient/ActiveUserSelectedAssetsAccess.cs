using System.Collections.Generic;
using Microsoft.SqlServer.Server;

namespace VSS.Hosted.VLCommon
{
  public static class ActiveUserSelectedAssetsAccess
  {
    public static void Save(long activeUserID, IEnumerable<long> selectedAssetIDs)
    {
      List<SqlDataRecord> data = SqlAccessMethods.Fill(selectedAssetIDs, "AssetID");

      StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_ActiveUserAssetSelection_Set");
      sp.AddInputTable("@AssetList", data, "tbl_AssetList");
      sp.AddInput("@ActiveUserID", activeUserID);
      SqlAccessMethods.ExecuteNonQuery(sp);    
		}

		public static void Save(long activeUserID, IEnumerable<long> selectedAssetIDs, long projectID)
		{
			List<SqlDataRecord> data = SqlAccessMethods.Fill(selectedAssetIDs, "AssetID");

			StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_ActiveUserAssetSelection_SetProjectID");
			sp.AddInputTable("@AssetList", data, "tbl_AssetList");
			sp.AddInput("@ActiveUserID", activeUserID);
			sp.AddInput("@ProjectID", projectID);

			SqlAccessMethods.ExecuteNonQuery(sp);
		}
   
  }
}
