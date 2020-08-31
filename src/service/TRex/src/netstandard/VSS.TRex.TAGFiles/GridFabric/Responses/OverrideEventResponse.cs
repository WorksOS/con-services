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

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Success);
      writer.WriteString(Message);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Success = reader.ReadBoolean();
        Message = reader.ReadString();
      }
    }
  }
}
