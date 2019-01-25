using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  /// <summary>
  /// The response returned from the StationOffset request executor that contains the response code and the set of
  /// sub grids extracted for the StationOffset report in question
  /// </summary>
  public class StationOffsetReportRequestResponse_ApplicationService : SubGridsPipelinedResponseBase
  {
    public ReportReturnCode ReturnCode;  // == TRaptorReportReturnCode
    public ReportType ReportType;        // == TRaptorReportType
    public List<StationOffsetReportDataRow_ApplicationService> StationOffsetReportDataRowList;

    public StationOffsetReportRequestResponse_ApplicationService()
    {
      Clear();
    }

    public void LoadStationOffsets(List<StationOffsetRow> stationOffsets)
    {
      var queryStations =
        from stationOffsetRow in stationOffsets
        group stationOffsetRow by stationOffsetRow.Station
        into newGroup
        orderby newGroup.Key
        select newGroup;
      foreach (var stationGroup in queryStations)
      {
        StationOffsetReportDataRowList.Add(new StationOffsetReportDataRow_ApplicationService
          (stationGroup.Key, stationGroup.ToList()));
      }
    }

    private void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.StationOffset;
      StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>();
    }
    

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      writer.WriteInt(StationOffsetReportDataRowList.Count);
      for (int i = 0; i < StationOffsetReportDataRowList.Count; i++)
      {
        StationOffsetReportDataRowList[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      ReturnCode = (ReportReturnCode)reader.ReadInt();
      ReportType = (ReportType)reader.ReadInt();
      var stationOffsetRowsCount = reader.ReadInt();
      StationOffsetReportDataRowList = new List<StationOffsetReportDataRow_ApplicationService>();
      for (int i = 0; i < stationOffsetRowsCount; i++)
      {
        var row = new StationOffsetReportDataRow_ApplicationService();
        row.FromBinary(reader);
        StationOffsetReportDataRowList.Add(row);
      }
    }
  }
}
