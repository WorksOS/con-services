using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class OverrideEventResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public bool Success { get; set; }

    public string Message { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public OverrideEventResponse()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Success);
      writer.WriteString(Message);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Success = reader.ReadBoolean();
      Message = reader.ReadString();
    }
  }
}
