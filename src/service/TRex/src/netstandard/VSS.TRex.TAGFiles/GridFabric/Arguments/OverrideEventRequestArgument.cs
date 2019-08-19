using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.TAGFiles.GridFabric.Arguments
{
  public class OverrideEventRequestArgument : BaseRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Determines if the overridden data is being added or removed.
    /// </summary>
    public bool Undo { get; set; } = false;
    
    /// <summary>
    /// The id of the project whose data is overridden.
    /// </summary>
    public Guid ProjectID { get; set; }

    /// <summary>
    /// The id of the machine whose data is overridden.
    /// </summary>
    public Guid AssetID { get; set; }

    /// <summary>
    /// The start time of the period to override
    /// </summary>
    public DateTime StartUTC { get; set; }
    /// <summary>
    /// The end time of the period to override
    /// </summary>
    public DateTime EndUTC { get; set; }

    /// <summary>
    /// The overriding design for the machine
    /// </summary>
    public string MachineDesignName { get; set; }

    /// <summary>
    /// The overriding layer for the machine
    /// </summary>
    public ushort? LayerID { get; set; }

    public OverrideEventRequestArgument()
    {
    }

    public OverrideEventRequestArgument(
      bool undo,
      Guid siteModelID,
      Guid assetID,
      DateTime startUTC,
      DateTime endUTC,
      string machineDesignName,
      ushort? layerID)
    {
      Undo = undo;
      ProjectID = siteModelID;
      AssetID = assetID;
      StartUTC = startUTC;
      EndUTC = endUTC;
      MachineDesignName = machineDesignName;
      LayerID = layerID;
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Undo);
      writer.WriteGuid(ProjectID);
      writer.WriteGuid(AssetID);
      writer.WriteLong(StartUTC.ToBinary());
      writer.WriteLong(EndUTC.ToBinary());
      writer.WriteString(MachineDesignName);
      writer.WriteBoolean(LayerID.HasValue);
      if (LayerID.HasValue)
        writer.WriteInt(LayerID.Value);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Undo = reader.ReadBoolean();
      ProjectID = reader.ReadGuid() ?? Guid.Empty;
      AssetID = reader.ReadGuid() ?? Guid.Empty;
      StartUTC = DateTime.FromBinary(reader.ReadLong());
      EndUTC = DateTime.FromBinary(reader.ReadLong());
      MachineDesignName = reader.ReadString();
      if (reader.ReadBoolean())
        LayerID = (ushort)reader.ReadInt();
    }

  }
}
