using System.Collections.Generic;
using System.Data.SqlClient;

namespace VSS.Hosted.VLCommon
{
  public static class AlertCountAccess
  {
    public static IDictionary<long, int> uspPub_GetAlertCountsForUserCustomer(long activeUserID, long customerID)
    {
      StoredProcDefinition proc = new StoredProcDefinition("NH_OP", "uspPub_GetAlertCountsForUserCustomer");
      proc.AddInput("@activeUserID", activeUserID);
      proc.AddInput("@customerID", customerID);

      return SqlReaderAccess.Read<long, int>(
        proc,
        delegate(SqlDataReader reader) { return SqlReaderAccess.GetLong(reader, "AssetID"); },
        delegate(SqlDataReader reader) { return SqlReaderAccess.GetInt(reader, "Count"); });
    }

  }
}
