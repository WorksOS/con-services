using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.Machines
{
  /// <summary>
  /// Defines all the metadata relating to a machine that has contributed in some way to a site model. These machine instances
  /// are relevant to individual sitemodels. There will be a machine instance within each site model that the machine has
  /// contributed to.
  /// </summary>
  public class Machine : IMachine
  {
    private const int READER_WRITER_VERSION_MACHINE = 1;

    public MachinesList Owner;

    public Guid ID { get; set; }

    public short InternalSiteModelMachineIndex { get; set; }

    public string Name { get; set; } = "";

    public byte MachineType { get; set; } = byte.MaxValue;

    public int DeviceType { get; set; } = int.MaxValue;

    public string MachineHardwareID { get; set; } = "";

    public bool IsJohnDoeMachine { get; set; }

    public double LastKnownX { get; set; } = Consts.NullDouble;

    public double LastKnownY { get; set; } = Consts.NullDouble;

    public DateTime LastKnownPositionTimeStamp { get; set; } = DateTime.MinValue;

    public string LastKnownDesignName { get; set; } = string.Empty;

    public ushort LastKnownLayerId { get; set; }

    private bool _compactionDataReported;

    /// <summary>
    /// Indicates if the machine has ever reported any compaction related data, such as CCV, MDP or CCA measurements
    /// </summary>
    public bool CompactionDataReported { get => _compactionDataReported; set => _compactionDataReported = _compactionDataReported | value; }

    public CompactionSensorType CompactionSensorType { get; set; } = CompactionSensorType.NoSensor;

    /// <summary>
    /// Determines if the type of this machine is one of the machine tyeps that supports compaction operations
    /// </summary>
    /// <returns></returns>
    public bool MachineIsCompactorType()
    {
      return MachineType == (byte)Types.MachineType.SoilCompactor ||
             MachineType == (byte)Types.MachineType.AsphaltCompactor ||
             MachineType == (byte)Types.MachineType.FourDrumLandfillCompactor;
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

    public Machine(MachinesList owner) : this()
    {
      Owner = owner;
    }

    public Machine(MachinesList owner,
                   string name,
                   string machineHardwareID,
                   byte machineType,
                   int deviceType,
                   Guid machineID,
                   short internalSiteModelMachineIndex,
                   bool isJohnDoeMachine
                   /* TODO: AConnectedMachineLevel : MachineLevelEnum*/) : this(owner)
    {
      Name = name;
      MachineHardwareID = machineHardwareID;
      MachineType = machineType;
      DeviceType = deviceType;

      ID = machineID;
      InternalSiteModelMachineIndex = internalSiteModelMachineIndex;
      IsJohnDoeMachine = isJohnDoeMachine;

      // TODO FConnectedMachineLevel:= AConnectedMachineLevel;
    }

    public void Assign(IMachine source)
    {
      Name = source.Name;
      MachineHardwareID = source.MachineHardwareID;
      CompactionSensorType = source.CompactionSensorType;
      // todo           CompactionRMVJumpThreshold = source.CompactionRMVJumpThreshold;
      // todo           UseMachineRMVThreshold = source.UseMachineRMVThreshold;
      // todo           OverrideRMVJumpThreshold = source.OverrideRMVJumpThreshold;
      DeviceType = source.DeviceType;
      CompactionDataReported = source.CompactionDataReported;
      // todo           ConnectedMachineLevel = source.ConnectedMachineLevel;
      MachineType = source.MachineType;
      IsJohnDoeMachine = source.IsJohnDoeMachine;
      LastKnownX = source.LastKnownX;
      LastKnownY = source.LastKnownY;
      LastKnownLayerId = source.LastKnownLayerId;
      LastKnownDesignName = source.LastKnownDesignName;
      LastKnownPositionTimeStamp = source.LastKnownPositionTimeStamp;

      //            Dirty = True;
    }

    /// <summary>
    /// Serializes machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(READER_WRITER_VERSION_MACHINE);

      writer.Write(ID.ToByteArray());
      writer.Write(InternalSiteModelMachineIndex);
      writer.Write(Name);
      writer.Write(MachineType);
      writer.Write(DeviceType);
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
      int version = reader.ReadInt32();
      if (version != READER_WRITER_VERSION_MACHINE)
        throw new TRexSerializationVersionException(READER_WRITER_VERSION_MACHINE, version);

      ID = reader.ReadGuid();
      InternalSiteModelMachineIndex = reader.ReadInt16();
      Name = reader.ReadString();
      MachineType = reader.ReadByte();
      DeviceType = reader.ReadInt32();
      MachineHardwareID = reader.ReadString();
      IsJohnDoeMachine = reader.ReadBoolean();
      LastKnownX = reader.ReadDouble();
      LastKnownY = reader.ReadDouble();
      LastKnownPositionTimeStamp = new DateTime(reader.ReadInt64());
      LastKnownDesignName = reader.ReadString();
      LastKnownLayerId = reader.ReadUInt16();
      CompactionDataReported = reader.ReadBoolean();
      CompactionSensorType = (CompactionSensorType)reader.ReadInt32();
    }
  }
}
