using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Server;
using System.Data;

namespace VSS.Hosted.VLCommon
{
  public static class PLDeviceAccess
  {

    public static void UpdatePLDeviceState(List<PLDevice> devices)
    {
      if (devices == null || devices.Count == 0)
      {
        return;
      }

      StoredProcDefinition sp = new StoredProcDefinition("NH_RAW", "usp_PLDevice_Update");
      List<SqlDataRecord> sqlData = new List<SqlDataRecord>();

      SqlMetaData[] metadata = new SqlMetaData[] { 
        new SqlMetaData("moduleCode", SqlDbType.VarChar, 50),
        new SqlMetaData("inAmericas", SqlDbType.Bit), 
        new SqlMetaData("globalgramEnabled", SqlDbType.Bit), 
        new SqlMetaData("satelliteNumber", SqlDbType.Int)
      };

      foreach (PLDevice device in devices)
      {
        SqlDataRecord record = new SqlDataRecord(metadata);
        record.SetValue(record.GetOrdinal("moduleCode"), device.ModuleCode);

        if (device.GlobalgramEnabled.HasValue)
        {
          record.SetValue(record.GetOrdinal("globalgramEnabled"), device.GlobalgramEnabled.Value);
        }

        record.SetValue(record.GetOrdinal("inAmericas"), device.InAmericas);

        if (device.SatelliteNumber.HasValue)
        {
          record.SetValue(record.GetOrdinal("satelliteNumber"), device.SatelliteNumber.Value);
        }
        sqlData.Add(record);
      }

      sp.AddInputTable("@devices", sqlData, "t_PLDevice");
      SqlAccessMethods.ExecuteNonQuery(sp);
    }

  }
}
