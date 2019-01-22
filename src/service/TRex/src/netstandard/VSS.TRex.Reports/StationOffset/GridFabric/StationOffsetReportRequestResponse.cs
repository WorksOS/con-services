using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.StationOffset.GridFabric
{
  /// <summary>
  /// The response returned from the StationOffset request executor that contains the response code and the set of
  /// subgrids extracted for the StationOffset report in question
  /// </summary>
  public class StationOffsetReportRequestResponse : SubGridsPipelinedResponseBase
  {
    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType;       // == TRaptorReportType
    public List<StationOffsetReportDataRow> StationOffsetReportDataRowList;

    public StationOffsetReportRequestResponse()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.StationOffset;
      StationOffsetReportDataRowList = new List<StationOffsetReportDataRow>();
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      writer.WriteInt(StationOffsetReportDataRowList.Count);
      for (int i = 0; i < StationOffsetReportDataRowList.Count; i++)
      {
        StationOffsetReportDataRowList[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      ReturnCode = (ReportReturnCode)reader.ReadInt();
      ReportType = (ReportType)reader.ReadInt();
      var stationOffsetRowsCount = reader.ReadInt();
      StationOffsetReportDataRowList = new List<StationOffsetReportDataRow>();
      for (int i = 0; i < stationOffsetRowsCount; i++)
      {
        var row = new StationOffsetReportDataRow();
        row.FromBinary(reader);
        StationOffsetReportDataRowList.Add(row);
      }
    }
  }
}
