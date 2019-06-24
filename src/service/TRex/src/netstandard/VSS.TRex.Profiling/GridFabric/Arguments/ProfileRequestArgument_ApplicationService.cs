using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Profiling.Models;


namespace VSS.TRex.Profiling.GridFabric.Arguments
{
  /// <summary>
  /// Defines the parameters required for a production data profile request argument on the application service node
  /// </summary>
  public class ProfileRequestArgument_ApplicationService : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public GridDataType ProfileTypeRequired { get; set; }
    public ProfileStyle ProfileStyle { get; set; }
    public WGS84Point StartPoint { get; set; } = new WGS84Point();
    public WGS84Point EndPoint { get; set; } = new WGS84Point();
    public bool PositionsAreGrid { get; set; }
    public OverrideParameters Overrides { get; set; } = new OverrideParameters();
    public bool ReturnAllPassesAndLayers { get; set; }

    /// <summary>
    /// The volume computation method to use when calculating summary volume information
    /// </summary>
    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;


    /// <summary>
    /// Constructs a default profile request argument
    /// </summary>
    public ProfileRequestArgument_ApplicationService()
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

      writer.WriteInt((int)ProfileTypeRequired);

      writer.WriteInt((int)ProfileStyle);

      writer.WriteBoolean(StartPoint != null);
      StartPoint?.ToBinary(writer);

      writer.WriteBoolean(EndPoint != null);
      EndPoint?.ToBinary(writer);

      writer.WriteBoolean(PositionsAreGrid);

      writer.WriteBoolean(ReturnAllPassesAndLayers);

      writer.WriteInt((int)VolumeType);

      writer.WriteBoolean(Overrides != null);
      Overrides?.ToBinary(writer);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ProfileTypeRequired = (GridDataType)reader.ReadInt();

      ProfileStyle = (ProfileStyle)reader.ReadInt();

      StartPoint = new WGS84Point();
      if (reader.ReadBoolean())
        StartPoint.FromBinary(reader);

      EndPoint = new WGS84Point();
      if (reader.ReadBoolean())
        EndPoint.FromBinary(reader);

      PositionsAreGrid = reader.ReadBoolean();

      ReturnAllPassesAndLayers = reader.ReadBoolean();

      VolumeType = (VolumeComputationType)reader.ReadInt();

      Overrides = new OverrideParameters();
      if (reader.ReadBoolean())
        Overrides.FromBinary(reader);
    }
  }
}
