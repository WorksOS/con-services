using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Types;

namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on cluster compute nodes
  /// </summary>
  public class ProfileRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public GridDataType ProfileTypeRequired { get; set; }

    public ProfileStyle ProfileStyle { get; set; }

    public XYZ[] NEECoords { get; set; } = new XYZ[0];

    public bool ReturnAllPassesAndLayers { get; set; }

    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    /// <summary>
    /// Constructs a default profile request argument
    /// </summary>
    public ProfileRequestArgument_ClusterCompute()
    {
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByte((byte)ProfileStyle);
      writer.WriteByte((byte)ProfileTypeRequired);

      var count = NEECoords?.Length ?? 0;
      writer.WriteInt(count);
      for (int i = 0; i < count; i++)
        NEECoords[i].ToBinary(writer);

      writer.WriteBoolean(ReturnAllPassesAndLayers);

      writer.WriteInt((int) VolumeType);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProfileStyle = (ProfileStyle) reader.ReadByte();
      ProfileTypeRequired = (GridDataType)reader.ReadByte();

      var count = reader.ReadInt();
      NEECoords = new XYZ[count];
      for (int i = 0; i < count; i++)
        NEECoords[i] = NEECoords[i].FromBinary(reader);

      ReturnAllPassesAndLayers = reader.ReadBoolean();
      VolumeType = (VolumeComputationType)reader.ReadInt();
    }
  }
}
