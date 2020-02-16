using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace VSS.Hosted.VLCommon
{
  public class PMGetAllintervalsAccess
  {
    public class AssetPMInterval
    {
      public long ID;
      public string Title { get; set; }
      public int TrackingTypeID { get; set; }
      public double? TrackingValueMilesOrHoursFirst { get; set; }
      public double? TrackingValueMilesOrHoursNext { get; set; }
      public bool IsCumulative { get; set; }
      public int Rank { get; set; }
    }

    public static IEnumerable<AssetPMInterval> GetAllIntervals(long assetID)
    {
      StoredProcDefinition sp = new StoredProcDefinition("NH_OP", "uspPub_GetAllPMIntervals");
      sp.AddInput("@assetID", assetID);      

      return SqlReaderAccess.Read<AssetPMInterval>(sp,
        delegate(SqlDataReader reader)
        {
          return new AssetPMInterval
          {
            ID = SqlReaderAccess.GetLong(reader, "ID"),
            Title = SqlReaderAccess.GetString(reader, "Title"),
            TrackingTypeID = SqlReaderAccess.GetInt(reader, "PMTrackingTypeID"),            
            IsCumulative = SqlReaderAccess.GetBool(reader, "IsCumulative"),
            Rank = SqlReaderAccess.GetInt(reader, "Rank"),
            TrackingValueMilesOrHoursFirst = SqlReaderAccess.GetNullableDouble(reader, "TrackingValueMilesOrHoursFirst"),
            TrackingValueMilesOrHoursNext = SqlReaderAccess.GetNullableDouble(reader, "TrackingValueMilesOrHoursNext")
          };
        });
    }
  }
}