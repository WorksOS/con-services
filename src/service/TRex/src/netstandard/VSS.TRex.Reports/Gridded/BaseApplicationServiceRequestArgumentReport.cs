using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Reports.Gridded
{
  /// <summary>
  /// Contains the set of report types required for request
  /// </summary>
  public class BaseApplicationServiceRequestArgumentReport : BaseApplicationServiceRequestArgument
  {
    public bool ReportElevation { get; set; }
    public bool ReportCutFill { get; set; }
    public bool ReportCmv { get; set; }
    public bool ReportMdp { get; set; }
    public bool ReportPassCount { get; set; }
    public bool ReportTemperature { get; set; }

    public BaseApplicationServiceRequestArgumentReport()
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

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteBoolean(ReportElevation);
      writer.WriteBoolean(ReportCmv);
      writer.WriteBoolean(ReportMdp);
      writer.WriteBoolean(ReportPassCount);
      writer.WriteBoolean(ReportTemperature);
      writer.WriteBoolean(ReportCutFill);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      ReportElevation = reader.ReadBoolean();
      ReportCmv = reader.ReadBoolean();
      ReportMdp = reader.ReadBoolean();
      ReportPassCount = reader.ReadBoolean();
      ReportTemperature = reader.ReadBoolean();
      ReportCutFill = reader.ReadBoolean();
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

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportElevation.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCutFill.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportCmv.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportMdp.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportPassCount.GetHashCode();
        hashCode = (hashCode * 397) ^ ReportTemperature.GetHashCode();
        return hashCode;
      }
    }

  }
}

