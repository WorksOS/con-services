using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data;

namespace VSS.Hosted.VLCommon
{
  public static class ServiceViewSaver
  {
    public static List<ServiceView> Save(List<ServiceView> data)
    {
      List<SqlDataRecord> param = FillDataTable(data.Distinct(new ServiceViewAccessComparer()).ToList());

      StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_ServiceView_Update");
      sp.AddInputTable("@serviceViews", param, "tbl_ServiceView");

      var result = new List<ServiceView>();
      using(var reader = SqlAccessMethods.ExecuteReader(sp))
      {
        while(reader.Read())
        {
          result.Add(new ServiceView
          {
            ID = reader.GetInt64(1),
            fk_ServiceID = reader.GetInt64(2),
            fk_CustomerID = reader.GetInt64(3),
            fk_AssetID = reader.GetInt64(4),
            StartKeyDate = reader.GetInt32(5),
            EndKeyDate = reader.GetInt32(6),
          });
        }
      }

      return result;
    }

    private static List<SqlDataRecord> FillDataTable(List<ServiceView> items)
    {
      List<SqlDataRecord> result = null;

      if (null == items)
        return result;

      foreach (ServiceView item in items)
      {
        SqlDataRecord record = new SqlDataRecord(new []{
          new SqlMetaData("ID", SqlDbType.BigInt),
          new SqlMetaData("fk_ServiceID", SqlDbType.BigInt),
          new SqlMetaData("fk_CustomerID", SqlDbType.BigInt),
          new SqlMetaData("fk_AssetID", SqlDbType.BigInt),
          new SqlMetaData("StartKeyDate", SqlDbType.Int),
          new SqlMetaData("EndKeyDate", SqlDbType.Int)
        });

        record.SetInt64(0, item.ID);
        record.SetInt64(1, item.fk_ServiceID);
        record.SetInt64(2, item.fk_CustomerID);
        record.SetInt64(3, item.fk_AssetID);
        record.SetInt32(4, item.StartKeyDate);
        record.SetInt32(5, item.EndKeyDate);

        if (null == result)
          result = new List<SqlDataRecord>();
        result.Add(record);

      }
      return result;
    }
  }

  internal class ServiceViewAccessComparer : IEqualityComparer<ServiceView>
  {
    public bool Equals(ServiceView a, ServiceView b)
    {
      if (a.fk_ServiceID == b.fk_ServiceID
            && a.fk_CustomerID == b.fk_CustomerID
            && a.fk_AssetID == b.fk_AssetID
            && a.ID == b.ID
            && a.StartKeyDate == b.StartKeyDate)
      {
        return true;
      }
      return false;
    }

    public int GetHashCode(ServiceView a)
    {
      if (a == null) return 0;
      return a.fk_ServiceID.GetHashCode() * a.fk_CustomerID.GetHashCode() * a.fk_AssetID.GetHashCode() * a.ID.GetHashCode() * a.StartKeyDate.GetHashCode();
    }
  }
}
