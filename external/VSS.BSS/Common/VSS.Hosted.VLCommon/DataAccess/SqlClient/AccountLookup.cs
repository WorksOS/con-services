using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace VSS.Hosted.VLCommon
{
  public static class AccountLookup
  {
      public static IEnumerable<string> GetParentAccountNames(long customerID)
    {
      StoredProcDefinition proc = new StoredProcDefinition("NH_OP", "uspPub_ParentAccounts_List");
      proc.AddInput("@customerID", customerID);

      using (SqlDataReader reader = SqlAccessMethods.ExecuteReader(proc))
      {
        while (reader.Read())
        {
            yield return reader.GetString(reader.GetOrdinal("name"));
        }
      }
    }
 
  }
}
