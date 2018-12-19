using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;
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

    public XYZ[] NEECoords { get; set; } = new XYZ[0];
    
    // todo LiftBuildSettings: TICLiftBuildSettings;

    public bool ReturnAllPassesAndLayers { get; set; }

    public VolumeComputationType VolumeType = VolumeComputationType.None;

    /// <summary>
    /// Constructs a default profile request argument
    /// </summary>
    public ProfileRequestArgument_ClusterCompute()
    {
    }

    /// <summary>
    /// Creates a new profile request argument initialized with the supplied parameters
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="nEECoords"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    public ProfileRequestArgument_ClusterCompute(GridDataType profileTypeRequired, XYZ[] nEECoords, bool returnAllPassesAndLayers, VolumeComputationType volumeType)
    {
      ProfileTypeRequired = profileTypeRequired;
      NEECoords = nEECoords;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
      VolumeType = volumeType;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteByte(VERSION_NUMBER);

      writer.WriteInt((int)ProfileTypeRequired);

      var count = NEECoords?.Length ?? 0;
      writer.WriteInt(count);
      for (int i = 0; i < count; i++)
        NEECoords[i].ToBinary(writer);

      writer.WriteBoolean(ReturnAllPassesAndLayers);

      writer.WriteInt((int)VolumeType);

    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      var version = reader.ReadByte();
      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      ProfileTypeRequired = (GridDataType)reader.ReadInt();

      var count = reader.ReadInt();
      NEECoords = new XYZ[count];
      for (int i = 0; i < count; i++)
        NEECoords[i] = NEECoords[i].FromBinary(reader);

      ReturnAllPassesAndLayers = reader.ReadBoolean();
      VolumeType = (VolumeComputationType)reader.ReadInt();

    }
  }
}
