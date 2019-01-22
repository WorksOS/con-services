using System;
using System.IO;

namespace VSS.TRex.Reports.Gridded
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class GriddedReportData : IEquatable<GriddedReportData>
  {
    public int NumberOfRows { get; set; }
    public GriddedReportDataRows Rows { get; set; }
    public bool ElevationReport { get; set; }
    public bool CutFillReport { get; set; }
    public bool CmvReport { get; set; }
    public bool MdpReport { get; set; }
    public bool PassCountReport { get; set; }
    public bool TemperatureReport { get; set; }


    public GriddedReportData()
    {
      Clear();
    }

    public void Clear()
    {
      Rows = new GriddedReportDataRows();
      NumberOfRows = Rows.Count;
      ElevationReport = false;
      CutFillReport = false;
      CmvReport = false;
      MdpReport = false;
      PassCountReport = false;
      TemperatureReport = false;
    }

    public void Write(BinaryWriter writer)
    {
      NumberOfRows = Rows.Count;
      writer.Write(NumberOfRows);
      Rows.Write(writer);
      writer.Write(ElevationReport);
      writer.Write(CutFillReport);
      writer.Write(CmvReport);
      writer.Write(MdpReport);
      writer.Write(PassCountReport);
      writer.Write(TemperatureReport);
    }

    public void Read(BinaryReader reader)
    {
      NumberOfRows = reader.ReadInt32();
      Rows.Read(reader, NumberOfRows);
      ElevationReport = reader.ReadBoolean();
      CutFillReport = reader.ReadBoolean();
      CmvReport = reader.ReadBoolean();
      MdpReport = reader.ReadBoolean();
      PassCountReport = reader.ReadBoolean();
      TemperatureReport = reader.ReadBoolean();
    }

    public bool Equals(GriddedReportData other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return NumberOfRows.Equals(other.NumberOfRows) &&
             Rows.Equals(other.Rows) &&
             ElevationReport.Equals(other.ElevationReport) &&
             CutFillReport.Equals(other.CutFillReport) &&
             CmvReport.Equals(other.CmvReport) &&
             MdpReport.Equals(other.MdpReport) &&
             PassCountReport.Equals(other.PassCountReport) &&
             TemperatureReport.Equals(other.TemperatureReport);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((GriddedReportData)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ NumberOfRows.GetHashCode();
        hashCode = (hashCode * 397) ^ Rows.GetHashCode();
        hashCode = (hashCode * 397) ^ ElevationReport.GetHashCode();
        hashCode = (hashCode * 397) ^ CutFillReport.GetHashCode();
        hashCode = (hashCode * 397) ^ CmvReport.GetHashCode();
        hashCode = (hashCode * 397) ^ MdpReport.GetHashCode();
        hashCode = (hashCode * 397) ^ PassCountReport.GetHashCode();
        hashCode = (hashCode * 397) ^ TemperatureReport.GetHashCode();
        return hashCode;
      }
    }

  }
}

