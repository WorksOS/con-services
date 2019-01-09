using System;
using System.IO;
using System.Text;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.TRex.Reports.Gridded
{
  /// <summary>
  /// Contains the prepared result for the client to consume.
  /// At present this contains only GriddedData, StationOffset will come later...
  /// 
  /// Note that this structure needs to be look like TRaptorReportsPackager
  ///   to be deserialized by it. One day... TRaptorReportsPackeger will
  ///   go away and GriddedReportResult could be deserialised in 3dp using it instead.
  /// </summary>
  public class GriddedReportResult : IEquatable<GriddedReportResult>
  {
    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType; // == TRaptorReportType
    public GriddedReportData GriddedData { get; set; }


    public GriddedReportResult()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.None;
      GriddedData = new GriddedReportData();
      GriddedData.Clear();
    }

    public GriddedReportResult(ReportType reportType)
    {
      ReportType = reportType;
      GriddedData = new GriddedReportData();
    }

    public byte[] Write()
    {
      using (var ms = new MemoryStream())
      {
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          bw.Write((int)ReturnCode);
          bw.Write((int)ReportType);

          GriddedData?.Write(bw);
        }

        return ms.ToArray();
      }
    }

    public Stream Read(byte[] byteArray)
    {
      using (var ms = new MemoryStream(byteArray))
      {
        using (var reader = new BinaryReader(ms, Encoding.UTF8, true))
        {
          ReturnCode = (ReportReturnCode)reader.ReadInt32();
          ReportType = (ReportType)reader.ReadInt32();

          GriddedData.Read(reader);
        }

        return ms;
      }
    }

    public bool Equals(GriddedReportResult other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ReturnCode.Equals(other.ReturnCode) &&
             ReportType.Equals(other.ReportType) &&
             GriddedData.Equals(other.GriddedData);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((GriddedReportResult)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ ReturnCode.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportType.GetHashCode();
        hashCode = (hashCode * 397) ^ GriddedData.GetHashCode();
        return hashCode;
      }
    }
  }
}
