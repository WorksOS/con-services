using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.CoordinateSystems.GridFabric.Responses
{
  /// <summary>
  /// The response state return from th add coordinate system operation
  /// </summary>
  public class AddCoordinateSystemResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Indicates overall success of the operation
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Succeeded);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Succeeded = reader.ReadBoolean();
      }
    }
  }
}
