using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common;

namespace VSS.TRex.QuantizedMesh.GridFabric.Responses
{
  public class ElevationGridResponse : SubGridsPipelinedResponseBase
  {
    private static byte VERSION_NUMBER = 1;

    public ReportReturnCode ReturnCode; // == TRaptorReportReturnCode
    public ReportType ReportType;       // == TRaptorReportType

    public ElevationGridResponse()
    {
      Clear();
    }

    public void Clear()
    {
      ReturnCode = ReportReturnCode.NoError;
      ReportType = ReportType.Gridded;
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
      }
    }
  }
}
