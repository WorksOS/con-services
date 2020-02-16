using System.Data;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Reflection;

namespace VSS.Hosted.VLCommon
{
  public class PMIntervalInstanceSaver
  {
      public static void Save(DataTable intervalInstances, List<long> assetIDs,  int resetModifiedRuntimehours=0, INH_OP ctx = null )
    {
      StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_PMIntervalInstance_Save");
      sp.AddInput("@InstanceList", intervalInstances);
      sp.AddInputTable("@assetIDs", SqlAccessMethods.Fill(assetIDs, "AssetID"), "tbl_AssetList");
      sp.AddInput("@ResetModifiedRuntimehours", resetModifiedRuntimehours);
      
      SqlAccessMethods.ExecuteNonQueryWithTransactionFromEntityConnection(sp, ctx != null && ctx.Connection != null ? (EntityConnection)ctx.Connection : null);
    }
  }
}
