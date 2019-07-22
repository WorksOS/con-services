using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.QuantizedMesh.GridFabric.Responses
{
  public class DummyQMResponse : QuantizedMeshResponse
  {
    private static byte VERSION_NUMBER = 1;

    public byte[] TileQMData { get; set; }

    public override IQuantizedMeshResponse AggregateWith(IQuantizedMeshResponse other)
    {
      return null;
    }

    public override void SetQMTile(byte[] qmTile)
    {
      TileQMData = qmTile;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(TileQMData != null);
      writer.WriteByteArray(TileQMData);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (reader.ReadBoolean())
        TileQMData = reader.ReadByteArray();
    }
  }
}
