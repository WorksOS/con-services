using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Reports.Gridded
{
  /// <summary>
  /// Contains the set of report types required for request
  /// </summary>
  public class BaseApplicationServiceRequestArgumentReport : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

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
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

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
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ReportElevation = reader.ReadBoolean();
        ReportCmv = reader.ReadBoolean();
        ReportMdp = reader.ReadBoolean();
        ReportPassCount = reader.ReadBoolean();
        ReportTemperature = reader.ReadBoolean();
        ReportCutFill = reader.ReadBoolean();
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(ReportElevation);
      writer.Write(ReportCutFill);
      writer.Write(ReportCmv);
      writer.Write(ReportMdp);
      writer.Write(ReportPassCount);
      writer.Write(ReportTemperature);
      writer.Write(RequestEmissionDateUtc.ToBinary());
    }

    public void Read(BinaryReader reader)
    {
      ReportElevation = reader.ReadBoolean();
      ReportCutFill = reader.ReadBoolean();
      ReportCmv = reader.ReadBoolean();
      ReportMdp = reader.ReadBoolean();
      ReportPassCount = reader.ReadBoolean();
      ReportTemperature = reader.ReadBoolean();
      RequestEmissionDateUtc = DateTime.FromBinary(reader.ReadInt64());
    }
  }
}

