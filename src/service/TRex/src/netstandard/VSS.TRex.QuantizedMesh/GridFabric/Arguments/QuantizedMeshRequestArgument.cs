using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.QuantizedMesh.GridFabric.Arguments
{
  public class QuantizedMeshRequestArgument : BaseApplicationServiceRequestArgument
  {

    private const byte VERSION_NUMBER = 1;
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int DisplayMode { get; set; }

    public QuantizedMeshRequestArgument()
    { }

    public QuantizedMeshRequestArgument(Guid projectUId, int x, int y, int z, IFilterSet filters, int displayMode)
    {
      // todo whats needed
      ProjectID = projectUId;
      X = x;
      Y = y;
      Z = z;
      Filters = filters;
      DisplayMode = displayMode;
    }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
      writer.WriteInt(X);
      writer.WriteInt(Y);
      writer.WriteInt(Z);
      writer.WriteInt(DisplayMode);
    }


    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
      X = reader.ReadInt();
      Y = reader.ReadInt();
      Z = reader.ReadInt();
      DisplayMode= reader.ReadInt();
    }

  }
}
