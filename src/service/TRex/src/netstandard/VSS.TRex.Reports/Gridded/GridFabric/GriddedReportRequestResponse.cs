using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The response returned from the Grid request executor that contains the response code and the set of
  /// subgrids extracted for the grid report in question
  /// </summary>
  public class GriddedReportRequestResponse : SubGridsPipelinedReponseBase
  {
    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType;       // == TRaptorReportType
    public List<GriddedReportDataRow> GriddedReportDataRowList;
    
    public GriddedReportRequestResponse()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.None;
      GriddedReportDataRowList = new List<GriddedReportDataRow>();
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
      writer.WriteInt(GriddedReportDataRowList.Count);
      for (int i = 0; i < GriddedReportDataRowList.Count; i++)
      {
        GriddedReportDataRowList[i].ToBinary(writer);
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
      var griddedRowsCount = reader.ReadInt();
      GriddedReportDataRowList = new List<GriddedReportDataRow>();
      for (int i = 0; i < griddedRowsCount; i++)
      {
        GriddedReportDataRowList[i].FromBinary(reader);
      }
    }
  }
}
