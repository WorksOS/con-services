using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Common;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Responses
{
  public class StationOffsetReportRequestResponse_ClusterCompute : SubGridsPipelinedResponseBase, IAggregateWith<StationOffsetReportRequestResponse_ClusterCompute>
  {
    private static byte VERSION_NUMBER = 1;

    public ReportReturnCode ReturnCode;  // == TRaptorReportReturnCode
    private ReportType ReportType;       // == TRaptorReportType
    public List<StationOffsetRow> StationOffsetRows;

    public StationOffsetReportRequestResponse_ClusterCompute()
    {
      Clear();
    }

    private void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.StationOffset;
      StationOffsetRows = new List<StationOffsetRow>();
    }

    /// <summary>
    /// Aggregate new stationOffsets into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    public StationOffsetReportRequestResponse_ClusterCompute AggregateWith(StationOffsetReportRequestResponse_ClusterCompute other)
    {
      this.StationOffsetRows.AddRange(other.StationOffsetRows);
      return this;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ReturnCode);
      writer.WriteInt((int)ReportType);
      writer.WriteInt(StationOffsetRows.Count);
      for (int i = 0; i < StationOffsetRows.Count; i++)
      {
        StationOffsetRows[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ReturnCode = (ReportReturnCode)reader.ReadInt();
      ReportType = (ReportType)reader.ReadInt();
      var griddedRowsCount = reader.ReadInt();
      StationOffsetRows = new List<StationOffsetRow>(griddedRowsCount);
      for (int i = 0; i < griddedRowsCount; i++)
      {
        var row = new StationOffsetRow();
        row.FromBinary(reader);
        StationOffsetRows.Add(row);
      }
    }
  }
}
