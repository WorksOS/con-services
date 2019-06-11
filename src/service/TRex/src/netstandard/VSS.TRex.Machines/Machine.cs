using System;
using System.IO;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Designs.TTM.Optimised;

namespace VSS.TRex.Machines
{
  /// <summary>
  /// Defines all the metadata relating to a machine that has contributed in some way to a site model. These machine instances
  /// are relevant to individual site models. There will be a machine instance within each site model that the machine has
  /// contributed to.
  /// </summary>
  public class Machine : IMachine, IBinaryReaderWriter
  {
    private const byte VERSION_NUMBER = 1;

    public Guid ID { get; set; }

    public short InternalSiteModelMachineIndex { get; set; }

    public string Name { get; set; } = "";

    public MachineType MachineType { get; set; } = MachineType.Unknown;

    public DeviceTypeEnum DeviceType { get; set; } = DeviceTypeEnum.MANUALDEVICE;

    public string MachineHardwareID { get; set; } = "";

    public bool IsJohnDoeMachine { get; set; }

    public double LastKnownX { get; set; } = Common.Consts.NullDouble;

    public double LastKnownY { get; set; } = Common.Consts.NullDouble;

    public DateTime LastKnownPositionTimeStamp { get; set; } = Common.Consts.MIN_DATETIME_AS_UTC;

    public string LastKnownDesignName { get; set; } = string.Empty;

    public ushort LastKnownLayerId { get; set; }

    private bool _compactionDataReported;

    /// <summary>
    /// Indicates if the machine has ever reported any compaction related data, such as CCV, MDP or CCA measurements
    /// </summary>
    public bool CompactionDataReported { get => _compactionDataReported; set => _compactionDataReported = _compactionDataReported | value; }

    public CompactionSensorType CompactionSensorType { get; set; } = CompactionSensorType.NoSensor;

    /// <summary>
    /// Determines if the type of this machine is one of the machine types that supports compaction operations
    /// </summary>
    /// <returns></returns>
    public bool MachineIsCompactorType()
    {
      return MachineType == MachineType.SoilCompactor ||
             MachineType == MachineType.AsphaltCompactor ||
             MachineType == MachineType.FourDrumLandfillCompactor;
    }

    public static bool MachineGearIsForwardGear(MachineGear gear)
    {
      return gear == MachineGear.Forward || gear == MachineGear.Forward2 || gear == MachineGear.Forward3 || gear == MachineGear.Forward4 || gear == MachineGear.Forward5;
    }

    public static bool MachineGearIsReverseGear(MachineGear gear)
    {
      return gear == MachineGear.Reverse || gear == MachineGear.Reverse2 || gear == MachineGear.Reverse3 || gear == MachineGear.Reverse4 || gear == MachineGear.Reverse5;
    }

    /// <summary>
    /// No args constructor for machine
    /// </summary>
    public Machine()
    {
    }

    public Machine(string name,
                   string machineHardwareID,
                   MachineType machineType,
                   DeviceTypeEnum deviceType,
                   Guid machineID,
                   short internalSiteModelMachineIndex,
                   bool isJohnDoeMachine) : this()
    {
      Name = name;
      MachineHardwareID = machineHardwareID;
      MachineType = machineType;
      DeviceType = deviceType;

      ID = machineID;
      InternalSiteModelMachineIndex = internalSiteModelMachineIndex;
      IsJohnDoeMachine = isJohnDoeMachine;
    }

    public void Assign(IMachine source)
    {
      ID = source.ID;
      InternalSiteModelMachineIndex = source.InternalSiteModelMachineIndex;
      Name = source.Name;
      MachineHardwareID = source.MachineHardwareID;
      CompactionSensorType = source.CompactionSensorType;
      // todo           CompactionRMVJumpThreshold = source.CompactionRMVJumpThreshold;
      // todo           UseMachineRMVThreshold = source.UseMachineRMVThreshold;
      // todo           OverrideRMVJumpThreshold = source.OverrideRMVJumpThreshold;
      DeviceType = source.DeviceType;
      CompactionDataReported = source.CompactionDataReported;
      MachineType = source.MachineType;
      IsJohnDoeMachine = source.IsJohnDoeMachine;
      LastKnownX = source.LastKnownX;
      LastKnownY = source.LastKnownY;
      LastKnownLayerId = source.LastKnownLayerId;
      LastKnownDesignName = source.LastKnownDesignName;
      LastKnownPositionTimeStamp = source.LastKnownPositionTimeStamp;
    }

    /// <summary>
    /// Serializes machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(ID.ToByteArray());
      writer.Write(InternalSiteModelMachineIndex);
      writer.Write(Name);
      writer.Write((byte)MachineType);
      writer.Write((int)DeviceType);
      writer.Write(MachineHardwareID);
      writer.Write(IsJohnDoeMachine);
      writer.Write(LastKnownX);
      writer.Write(LastKnownY);
      writer.Write(LastKnownPositionTimeStamp.ToBinary());
      writer.Write(LastKnownDesignName);
      writer.Write(LastKnownLayerId);
      writer.Write(CompactionDataReported);
      writer.Write((int)CompactionSensorType);
    }

    /// <summary>
    /// Deserializes the machine using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      ID = reader.ReadGuid();
      InternalSiteModelMachineIndex = reader.ReadInt16();
      Name = reader.ReadString();
      MachineType = (MachineType)reader.ReadByte();
      DeviceType = (DeviceTypeEnum)reader.ReadInt32();
      MachineHardwareID = reader.ReadString();
      IsJohnDoeMachine = reader.ReadBoolean();
      LastKnownX = reader.ReadDouble();
      LastKnownY = reader.ReadDouble();
      LastKnownPositionTimeStamp = DateTime.FromBinary(reader.ReadInt64());
      LastKnownDesignName = reader.ReadString();
      LastKnownLayerId = reader.ReadUInt16();
      CompactionDataReported = reader.ReadBoolean();
      CompactionSensorType = (CompactionSensorType)reader.ReadInt32();
    }
  }
}
