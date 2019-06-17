using System;
using Apache.Ignite.Core.Binary;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Types;

namespace VSS.TRex.Cells
{
  /// <summary>
  ///  Stores the values of various 'event' values at the time the pass information was captured.
  /// </summary>
  public struct CellEvents
  {
    /// <summary>
    /// Value to indicate a null design name ID
    /// </summary>
    public const int NoDesignNameID = 0;

    /// <summary>
    /// Null value for ID of Layer selected by the machine control system operator
    /// </summary>
    public const ushort NullLayerID = ushort.MaxValue;

    /// <summary>
    /// The elevation mapping mode in use on the machine
    /// </summary>
    public ElevationMappingMode EventElevationMappingMode;

    /// <summary>
    /// Is the machine is a defined avoidance zone
    /// </summary>
    public byte EventInAvoidZoneState;

    /// <summary>
    /// The ID of the loaded design on the machine control system
    /// </summary>
    public int EventDesignNameID;

    /// <summary>
    /// Compaction drum vibration state
    /// </summary>
    public VibrationState EventVibrationState;

    /// <summary>
    /// Automatics control state for vibratory drums
    /// </summary>
    public AutoVibrationState EventAutoVibrationState;

    /// <summary>
    /// Transmission gear the machine is in
    /// </summary>
    public MachineGear EventMachineGear;

    /// <summary>
    /// Resonance Meter Threshold value above which decoupling exists at the drum/ground boundary
    /// </summary>
    public short EventMachineRMVThreshold;

    /// <summary>
    /// Automatics controls state the machine control system is operating in
    /// </summary>
    public AutomaticsType EventMachineAutomatics;

    /// <summary>
    /// Positioning technology used to calculate machine position
    /// </summary>
    public PositioningTech PositioningTechnology;

    /// <summary>
    /// GPS error tolerance metric published by machine control system
    /// </summary>
    public ushort GPSTolerance;

    /// <summary>
    /// GPS accuracy band (find/medium/coarse) for positioning reported by the machine control system
    /// </summary>
    public GPSAccuracy GPSAccuracy;

    /// <summary>
    /// The date at which the closest preceding Map Reset event was triggered on a machine control system
    /// </summary>
    public DateTime MapReset_PriorDate;

    /// <summary>
    /// The ID of the design loaded at the time of the closest preceding Map Reset event triggered on a machine control system
    /// </summary>
    public int MapReset_DesignNameID;

    /// <summary>
    /// Layer ID selected by the machine control system operator
    /// </summary>
    public ushort LayerID;

    /// <summary>
    /// Grouped flags related to compaction and earthworks state
    /// </summary>
    public byte EventFlags;

    public void Clear()
    {
      EventDesignNameID = NoDesignNameID;
      EventVibrationState = VibrationState.Invalid;
      EventAutoVibrationState = AutoVibrationState.Unknown;
      EventFlags = 0;
      EventMachineGear = MachineGear.Neutral;
      EventMachineRMVThreshold = CellPassConsts.NullRMV;
      EventMachineAutomatics = AutomaticsType.Unknown;
      EventElevationMappingMode = ElevationMappingMode.LatestElevation;
      EventInAvoidZoneState = 0;

      MapReset_PriorDate = CellPassConsts.NullTime;
      MapReset_DesignNameID = NoDesignNameID;

      GPSAccuracy = GPSAccuracy.Unknown;
      GPSTolerance = CellPassConsts.NullGPSTolerance;
      PositioningTechnology = PositioningTech.Unknown;

      LayerID = NullLayerID;
    }

    /// <summary>
    /// Assign the contents of one Cell Events instance to this instance
    /// </summary>
    /// <param name="source"></param>
    public void Assign(CellEvents source)
    {
      EventDesignNameID = source.EventDesignNameID;
      EventVibrationState = source.EventVibrationState;
      EventAutoVibrationState = source.EventAutoVibrationState;
      EventFlags = source.EventFlags;
      EventMachineGear = source.EventMachineGear;
      EventMachineRMVThreshold = source.EventMachineRMVThreshold;
      EventMachineAutomatics = source.EventMachineAutomatics;
      EventElevationMappingMode = source.EventElevationMappingMode;
      EventInAvoidZoneState = source.EventInAvoidZoneState;

      MapReset_PriorDate = source.MapReset_PriorDate;
      MapReset_DesignNameID = source.MapReset_DesignNameID;

      GPSAccuracy = source.GPSAccuracy;
      GPSTolerance = source.GPSTolerance;
      PositioningTechnology = source.PositioningTechnology;

      LayerID = source.LayerID;
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte((byte)EventElevationMappingMode);
      writer.WriteByte(EventInAvoidZoneState);
      writer.WriteInt(EventDesignNameID);
      writer.WriteByte((byte)EventVibrationState);
      writer.WriteByte((byte)EventAutoVibrationState);
      writer.WriteByte((byte)EventMachineGear);
      writer.WriteShort(EventMachineRMVThreshold);
      writer.WriteByte((byte)EventMachineAutomatics);
      writer.WriteByte((byte)PositioningTechnology);
      writer.WriteInt(GPSTolerance);
      writer.WriteByte((byte)GPSAccuracy);
      writer.WriteLong(MapReset_PriorDate.ToBinary());
      writer.WriteInt(MapReset_DesignNameID);
      writer.WriteInt(LayerID);
      writer.WriteByte(EventFlags);
    }

    /// <summary>
    /// Serializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      EventElevationMappingMode = (ElevationMappingMode)reader.ReadByte();
      EventInAvoidZoneState = reader.ReadByte();
      EventDesignNameID = reader.ReadInt();
      EventVibrationState = (VibrationState)reader.ReadByte();
      EventAutoVibrationState = (AutoVibrationState)reader.ReadByte();
      EventMachineGear = (MachineGear)reader.ReadByte();
      EventMachineRMVThreshold = reader.ReadShort();
      EventMachineAutomatics = (AutomaticsType)reader.ReadByte();
      PositioningTechnology = (PositioningTech)reader.ReadByte();
      GPSTolerance = (ushort)reader.ReadInt();
      GPSAccuracy = (GPSAccuracy)reader.ReadByte();
      MapReset_PriorDate = DateTime.FromBinary(reader.ReadLong());
      MapReset_DesignNameID = reader.ReadInt();
      LayerID = (ushort)reader.ReadInt();
      EventFlags = reader.ReadByte();
    }
  }
}
