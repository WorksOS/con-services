using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;

namespace VSS.TRex.Cells
{
  /* This unit implements the full pass coverage cells for the Intelligent
      Compaction data grid.

      This grid is similar to the grid used to store the as built terrain surface,
      but differs in that it maintains a history of all passes over the grid.
      In fact, the grid really only serves as a form of spatial index for the epoch
      internal data representing a compactors movements across the site.

      <CellPass> Represents a spatially location square area of ground over which
      one or more compactor machines have passed over. Each pass is represented as
      an entry in the<Passes> property which is a list of all <TICEpochInterval>s
      whose extents overly the center of the cell.While the cell is spatially
      located, it does not in itself know where it is. Location is determined via
      context in the main IC grid.
  */

  /// <summary>
  /// Describes all the 'per-cell' state recorded for a pass recorded by a machine over a cell.
  /// </summary>
  public struct CellPass
  {
    /// <summary>
    /// GPSModeStore stores the GPS mode in the lowest 4 bits of the GPSModeStore byte. The remaining 4 bits are
    /// available for use, probably as flags.
    /// </summary>
    public byte GPSModeStore;

    /// <summary>
    /// The GPS mode in effect when the cell pass was measured
    /// </summary>
    public GPSMode gpsMode
    {
      get => (GPSMode)(GPSModeStore & CellPassConsts.GPS_MODE_STORE_BIT_MASK); 
      set => GPSModeStore = (byte)((GPSModeStore & ~CellPassConsts.GPS_MODE_STORE_BIT_MASK) | ((byte)value & CellPassConsts.GPS_MODE_STORE_BIT_MASK)); 
    }

    /// <summary>
    /// Is the cell pass a 'half' pass made by a machine with two implements (eg: an asphalt roller)
    /// </summary>
    public bool HalfPass
    {
      get => (GPSModeStore & (1 << (int)GPSFlagBits.GPSSBitHalfPass)) != 0; 
      set => GPSModeStore = (byte)(value ? GPSModeStore | 1 << (int)GPSFlagBits.GPSSBitHalfPass : GPSModeStore & ~(1 << (int)GPSFlagBits.GPSSBitHalfPass)); 
    }

    /// <summary>
    /// The type of the pass (front, rear, track or wheel)
    /// </summary>
    public PassType PassType
    {
      get => PassTypeHelper.GetPassType(GPSModeStore); 
      set => GPSModeStore = PassTypeHelper.SetPassType(GPSModeStore, value); 
    }

    /// <summary>
    /// The external descriptor for a machine within a project. This is immutable and once a machine is created in the project. The machine
    /// may always be referred to via this descriptor
    /// </summary>
    /// Note: This is removed in favor of CellPasses only ever containing the internal site model machine index
    /// public long MachineID { get; set; }

    /// <summary>
    /// The internal descriptor for a machine within a project. This is volatile and is not guaranteed to be the same value between references by
    /// an 'external' consumer of the project
    /// </summary>
    public short InternalSiteModelMachineIndex { get; set; }

    /// <summary>
    /// The measured height (actually grid elevation from NEE) fo the cell pass
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// The UTC time at which the cell pass was measured
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///  The measured CCV (CMV - Compaction Meter Value) for this pass
    /// </summary>
    public short CCV { get; set; }

    /// <summary>
    /// The latency of CMR packets over the radio network
    /// </summary>
    public byte RadioLatency { get; set; }

    /// <summary>
    /// The Resonance Metered Value for this pass
    /// </summary>
    public short RMV { get; set; }

    /// <summary>
    /// Frequency of drum for this pass
    /// </summary>
    public ushort Frequency { get; set; }

    /// <summary>
    /// Amplitude of drum for this pass
    /// </summary>
    public ushort Amplitude { get; set; }

    /// <summary>
    /// The speed of the machine, in centimeters per second, for this pass
    /// </summary>
    public ushort MachineSpeed { get; set; }

    /// <summary>
    /// Temperature of material being compacted (asphalt) for this pass
    /// </summary>
    public ushort MaterialTemperature { get; set; }

    /// <summary>
    /// The measured Machine Drive Power value, for this pass
    /// </summary>
    public short MDP { get; set; }

    /// <summary>
    /// The measured CCA (Caterpillar Compaction Algorithm) value, for this pass
    /// </summary>
    public byte CCA { get; set; }

    /// <summary>
    /// Initialise all attributes of a cell pass to null
    /// </summary>
    public void Clear()
    {
      GPSModeStore        = 0;

      Time                = CellPassConsts.NullTime;

      Height              = CellPassConsts.NullHeight;
      InternalSiteModelMachineIndex = CellPassConsts.NullInternalSiteModelMachineIndex;
      gpsMode             = CellPassConsts.NullGPSMode;
      CCV                 = CellPassConsts.NullCCV;
      RadioLatency        = CellPassConsts.NullRadioLatency;
      RMV                 = CellPassConsts.NullRMV;
      Frequency           = CellPassConsts.NullFrequency;
      Amplitude           = CellPassConsts.NullAmplitude;
      MaterialTemperature = CellPassConsts.NullMaterialTemperatureValue;
      MDP                 = CellPassConsts.NullMDP;
      MachineSpeed        = CellPassConsts.NullMachineSpeed;
      CCA                 = CellPassConsts.NullCCA;
    }

    /// <summary>
    /// Extract the machine ID and time from a cell pass in a single operation
    /// </summary>
    /// <param name="internalSiteModelMachineIndex"></param>
    /// <param name="time"></param>
    public void MachineIDAndTime(out short internalSiteModelMachineIndex, out DateTime time)
    {
      internalSiteModelMachineIndex = InternalSiteModelMachineIndex;
      time = Time;
    }

    /// <summary>
    /// Ensure all fields that should have null values are set to null if the vibration state is off
    /// </summary>
    public void SetFieldsForVibeStateOff()
    {
      CCV = CellPassConsts.NullCCV;
      RMV = CellPassConsts.NullRMV;
      Frequency = CellPassConsts.NullFrequency;
      Amplitude = CellPassConsts.NullAmplitude;
    }

    /// <summary>
    /// Tests if the content opf this cell pass is equal to the content of another cell pass.
    /// Note: This does nto implement IEquatable or similar interface to retain the pure
    /// struct semantics of CellPass
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(CellPass other)
    {
      return GPSModeStore == other.GPSModeStore &&
             InternalSiteModelMachineIndex == other.InternalSiteModelMachineIndex &&
             Height == other.Height &&
             Time == other.Time &&
             CCV == other.CCV &&
             RadioLatency == other.RadioLatency &&
             RMV == other.RMV &&
             Frequency == other.Frequency &&
             Amplitude == other.Amplitude &&
             MaterialTemperature == other.MaterialTemperature &&
             MachineSpeed == other.MachineSpeed &&
             MDP == other.MDP &&
             CCA == other.CCA;
    }

    /// <summary>
    /// Produce a human readable text version of the information contained in a cell pass
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"Time:{Time:yyyy-MM-dd hh-mm-ss.fff} InternalMachineID:{InternalSiteModelMachineIndex}, Height:{Height}, CCV:{CCV}, RadioLatency:{RadioLatency}, RMV:{RMV}, GPSMode:{gpsMode}, Freq:{Frequency}, Amp:{Amplitude}, Temperature:{MaterialTemperature}, Speed:{MachineSpeed}, MDP:{MDP}, CCA:{CCA}";
    }

    /// <summary>
    /// Assign the content of another cell pass to this cell pass
    /// </summary>
    /// <param name="Pass"></param>
    public void Assign(CellPass Pass)
    {
      GPSModeStore = Pass.GPSModeStore;

      InternalSiteModelMachineIndex = Pass.InternalSiteModelMachineIndex;
      Height = Pass.Height;
      Time = Pass.Time;
      CCV = Pass.CCV;
      RadioLatency = Pass.RadioLatency;
      RMV = Pass.RMV;
      Frequency = Pass.Frequency;
      Amplitude = Pass.Amplitude;
      MaterialTemperature = Pass.MaterialTemperature;
      MachineSpeed = Pass.MachineSpeed;
      MDP = Pass.MDP;
      CCA = Pass.CCA;
    }

    /// <summary>
    /// Emit the content of the cell pass into a binary stream represented by a BinaryWriter
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(GPSModeStore);
      writer.Write(InternalSiteModelMachineIndex);
      writer.Write(Height);
      writer.Write(Time.ToBinary());
      writer.Write(CCV);
      writer.Write(RadioLatency);
      writer.Write(RMV);
      writer.Write(Frequency);
      writer.Write(Amplitude);
      writer.Write(MaterialTemperature);
      writer.Write(MachineSpeed);
      writer.Write(MDP);
      writer.Write(CCA);
    }

    /// <summary>
    /// Read the context of a cell pass from a binary stream represented by a BinaryReader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      GPSModeStore = reader.ReadByte();
      InternalSiteModelMachineIndex = reader.ReadInt16();
      Height = reader.ReadSingle();
      Time = DateTime.FromBinary(reader.ReadInt64());
      CCV = reader.ReadInt16();
      RadioLatency = reader.ReadByte();
      RMV = reader.ReadInt16();
      Frequency = reader.ReadUInt16();
      Amplitude = reader.ReadUInt16();
      MaterialTemperature = reader.ReadUInt16();
      MachineSpeed = reader.ReadUInt16();
      MDP = reader.ReadInt16();
      CCA = reader.ReadByte();
    }

    /// <summary>
    /// Serializes content of the cell to the writer
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(GPSModeStore);
      writer.WriteShort(InternalSiteModelMachineIndex);
      writer.WriteFloat(Height);
      writer.WriteLong(Time.ToBinary());
      writer.WriteShort(CCV);
      writer.WriteByte(RadioLatency);
      writer.WriteShort(RMV);
      writer.WriteInt(Frequency);
      writer.WriteInt(Amplitude);
      writer.WriteInt(MaterialTemperature);
      writer.WriteInt(MachineSpeed);
      writer.WriteShort(MDP);
      writer.WriteByte(CCA);
    }

    /// <summary>
    /// Deserializes content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      GPSModeStore = reader.ReadByte();
      InternalSiteModelMachineIndex = reader.ReadShort();
      Height = reader.ReadFloat();
      Time = DateTime.FromBinary(reader.ReadLong());
      CCV = reader.ReadShort();
      RadioLatency = reader.ReadByte();
      RMV = reader.ReadShort();
      Frequency = (ushort)reader.ReadInt();
      Amplitude = (ushort)reader.ReadInt();
      MaterialTemperature = (ushort)reader.ReadInt();
      MachineSpeed = (ushort)reader.ReadInt();
      MDP = reader.ReadShort();
      CCA = reader.ReadByte();
    }
  }
}
