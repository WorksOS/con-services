using System.IO;

namespace VSS.Productivity3D.Models.Models.Reports
{
  /// <summary>
  /// Contains the set of report types required for request
  /// </summary>
  public class ReportTypeData 
  {
    public bool ReportElevation { get; set; }
    public bool ReportCutFill { get; set; }
    public bool ReportCmv { get; set; }
    public bool ReportMdp { get; set; }
    public bool ReportPassCount { get; set; }
    public bool ReportTemperature { get; set; }

    public ReportTypeData()
    {
      Clear();
    }

    public void Clear()
    {
      ReportElevation = false;
      ReportCutFill = false;
      ReportCmv = false;
      ReportMdp = false;
      ReportPassCount = false;
      ReportTemperature = false;
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(ReportElevation);
      writer.Write(ReportCutFill);
      writer.Write(ReportCmv);
      writer.Write(ReportMdp);
      writer.Write(ReportPassCount);
      writer.Write(ReportTemperature);
    }

    public void Read(BinaryReader reader)
    {
      ReportElevation = reader.ReadBoolean();
      ReportCutFill = reader.ReadBoolean();
      ReportCmv = reader.ReadBoolean();
      ReportMdp = reader.ReadBoolean();
      ReportPassCount = reader.ReadBoolean();
      ReportTemperature = reader.ReadBoolean();
    }
  }
}

