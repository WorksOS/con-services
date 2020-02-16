using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace VSS.Hosted.VLCommon
{
  public class AddressSaver
  {
    public class AddressData
    {
      public long ID { get; set; }
      public double Latitude { get; set; }
      public double Longitude { get; set; }
      public string Address { get; set; }
      public string State { get; set; }
    }

    public static void Save(DataTable dataAddress, long maxHLID)
    {
      StoredProcDefinition sp = new StoredProcDefinition("NH_RPT", "usp_DimAddress_Save");
      sp.AddInput("@data", dataAddress);
      sp.AddInput("@BookMark", maxHLID);
      SqlAccessMethods.ExecuteNonQuery(sp);
    }

    public static IEnumerable<AddressData> GetNextLocationBatch()
    {
      StoredProcDefinition sp = new StoredProcDefinition("NH_RPT", "uspPub_GetNextLocationBatch");

      return SqlReaderAccess.Read<AddressData>(sp,
        delegate(SqlDataReader reader)
        {
          return new AddressData
          {
            ID = SqlReaderAccess.GetLong(reader, "ID"),
            Latitude = SqlReaderAccess.GetDouble(reader, "Latitude"),
            Longitude = SqlReaderAccess.GetDouble(reader, "Longitude")
          };
        });
    }
  }
}
