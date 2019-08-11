using System.IO;
using System.Text;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Contains the prepared Station Offset Report result for the client to consume.
  /// </summary>
  public class StationOffsetReportResultPackager
  {
    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType; // == TRaptorReportType
    public StationOffsetReportData GriddedData { get; set; }
    
    public StationOffsetReportResultPackager()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.None;
      GriddedData = new StationOffsetReportData();
      GriddedData.Clear();
    }

    public StationOffsetReportResultPackager(ReportType reportType)
    {
      ReportType = reportType;
      GriddedData = new StationOffsetReportData();
    }

    public byte[] Write()
    {
      using (var ms = new MemoryStream())
      {
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          bw.Write((int)ReturnCode);
          bw.Write((int)ReportType);

          GriddedData.Write(bw);
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
  }
}
