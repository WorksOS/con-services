using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// The response returned from the Grid request executor that contains the response code and the set of
  /// sub grids extracted for the grid report in question
  /// </summary>
  public class GriddedReportRequestResponse : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

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
      ReportType = ReportType.Gridded;
      GriddedReportDataRowList = new List<GriddedReportDataRow>();
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      writer.WriteInt(GriddedReportDataRowList.Count);
      for (int i = 0; i < GriddedReportDataRowList.Count; i++)
      {
        GriddedReportDataRowList[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ReturnCode = (ReportReturnCode) reader.ReadInt();
        ReportType = (ReportType) reader.ReadInt();
        var griddedRowsCount = reader.ReadInt();
        GriddedReportDataRowList = new List<GriddedReportDataRow>();
        for (int i = 0; i < griddedRowsCount; i++)
        {
          var row = new GriddedReportDataRow();
          row.FromBinary(reader);
          GriddedReportDataRowList.Add(row);
        }
      }
    }
  }
}
