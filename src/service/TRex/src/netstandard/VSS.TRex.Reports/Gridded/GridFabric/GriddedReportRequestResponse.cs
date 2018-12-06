using System;
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
  public class GriddedReportRequestResponse : SubGridsPipelinedReponseBase, IEquatable<GriddedReportRequestResponse>
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

    public bool Equals(GriddedReportRequestResponse other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      var result = base.Equals(other) &&
                   ReturnCode.Equals(other.ReturnCode) &&
                   ReportType.Equals(other.ReportType);
      if (!result)
        return false;

      if ((GriddedReportDataRowList == null && other.GriddedReportDataRowList != null) ||
          (GriddedReportDataRowList != null && other.GriddedReportDataRowList == null) ||
          (GriddedReportDataRowList?.Count != other.GriddedReportDataRowList?.Count))
        return false;

      for (int i = 0; i < GriddedReportDataRowList.Count; i++)
      {
        if (!GriddedReportDataRowList[i].Equals(other.GriddedReportDataRowList[i]))
          return false;
      }

      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((GriddedReportRequestResponse) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ ReturnCode.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportType.GetHashCode();
        hashCode = (hashCode * 397) ^ GriddedReportDataRowList.GetHashCode();
        return hashCode;
      }
    }
  }
}
