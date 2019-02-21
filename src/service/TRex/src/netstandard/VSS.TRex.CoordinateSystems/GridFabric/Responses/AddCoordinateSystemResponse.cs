using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;

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
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);

      writer.WriteBoolean(Succeeded);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      byte version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      Succeeded = reader.ReadBoolean();
    }
  }
}
