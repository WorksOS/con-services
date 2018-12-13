using System;
using System.IO;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;

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
    /// Helper class that maps the two bites in the GPSMode byte to the four pass type values
    /// </summary>
    public static class PassTypeHelper
    {
      /// <summary>
      /// Sets the appropriate bits in the GPSModeStore corresponding to the desired pass type
      /// </summary>
      /// <param name="value"></param>
      /// <param name="_passType"></param>
      /// <returns></returns>
      public static byte SetPassType(byte value, PassType _passType)
      {
        byte result = value;

        switch (_passType)
        {
          case PassType.Front: // val 0
						{
              result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit6);
              result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit7);
              break;
            }
          case PassType.Rear: // val 1
            {
							result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit6);
							result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit7);
							break;
            }
          case PassType.Track: // val 2
            {
              result = BitFlagHelper.BitOff(result, (int)GPSFlagBits.GPSSBit6);
              result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit7);
              break;
            }
          case PassType.Wheel: // val 3
            {
              result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit6);
              result = BitFlagHelper.BitOn(result, (int)GPSFlagBits.GPSSBit7);
              break;
            }
          default:
            {
              throw new ArgumentException($"Unknown pass type supplied to SetPassType {_passType}", "_passType");
            }
        }

        return result;
      }

      /// <summary>
      /// Extracts the PassType enum value from the bi flags used to represent it
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      public static PassType GetPassType(byte value)
      {
        byte testByte = 0;

        if ((value & (1 << (int)GPSFlagBits.GPSSBit6)) != 0)
        {
          testByte = 1;
        }
        if ((value & (1 << (int)GPSFlagBits.GPSSBit7)) != 0)
        {
          testByte += 2;
        }

        return (PassType)testByte;
      }

      /// <summary>
      /// Determines if a PassType encoded in the PassType enum is a member of the 
      /// PassTypeSet flag enum
      /// </summary>
      /// <param name="PassTypeSet"></param>
      /// <param name="PassType"></param>
      /// <returns></returns>
      public static bool PassTypeSetContains(PassTypeSet PassTypeSet, PassType PassType)
      {
        return ((int)PassTypeSet & (1 << (int) PassType)) != 0;
      }
    }

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
      get { return (GPSMode)(GPSModeStore & 0x0F); }
      set { GPSModeStore = (byte)((GPSModeStore & 0xF0) | ((byte)value & 0x0F)); }
    }

    /// <summary>
    /// Is the cell pass a 'half' pass made by a machine with two implements (eg: an asphalt roller)
    /// </summary>
    public bool HalfPass
    {
      get { return (GPSModeStore & (1 << (int)GPSFlagBits.GPSSBitHalfPass)) != 0; }
      set { GPSModeStore = (byte)(value ? GPSModeStore | 1 << (int)GPSFlagBits.GPSSBitHalfPass : GPSModeStore & ~(1 << (int)GPSFlagBits.GPSSBitHalfPass)); }
    }

    /// <summary>
    /// The type of the pass (front, rear, track or wheel)
    /// </summary>
    public PassType PassType
    {
      get { return PassTypeHelper.GetPassType(GPSModeStore); }
      set { GPSModeStore = PassTypeHelper.SetPassType(GPSModeStore, value); }
    }

    /// <summary>
    /// The external descriptor for a machine within a project. This is immutable and once a machine is created in the project. The machine
    /// may always be referred to via this descriptor
    /// </summary>
    /// Note: This is removed in favour of CellPasses only ever containing the internal sitemodel machine index
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
    /// The measured CCA (Caterpillar COmpaction Algorithm) value, for this pass
    /// </summary>
    public byte CCA { get; set; }

    /// <summary>
    /// Initialise all attributes of a cell pass to null
    /// </summary>
    public void Clear()
    {
      GPSModeStore = 0;

      Time = CellPassConsts.NullTime;

      Height = CellPassConsts.NullHeight;
      // MachineID = NullMachineID;
      InternalSiteModelMachineIndex = CellPassConsts.NullInternalSiteModelMachineIndex;
      gpsMode = CellPassConsts.NullGPSMode;
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
    public void MachineIDAndTime(/*out long machineID, */ out short internalSiteModelMachineIndex, out DateTime time)
    {
      //machineID = MachineID;
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
    /// Produce a human readable text version of the information contained in a cell pass
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      //return  $"Time:{Time} MachineID:{MachineID}, Height:{Height}, CCV:{CCV}, RadioLatency:{RadioLatency}, RMV:{RMV}, GPSMode:{gpsMode}, Freq:{Frequency}, Amp:{Amplitude}, Temperature:{MaterialTemperature}, Speed:{MachineSpeed}, MDP:{MDP}, CCA:{CCA}";
      return $"Time:{Time} InternalMachineID:{InternalSiteModelMachineIndex}, Height:{Height}, CCV:{CCV}, RadioLatency:{RadioLatency}, RMV:{RMV}, GPSMode:{gpsMode}, Freq:{Frequency}, Amp:{Amplitude}, Temperature:{MaterialTemperature}, Speed:{MachineSpeed}, MDP:{MDP}, CCA:{CCA}";
    }

    /// <summary>
    /// Assign the content of another cell pass to this cell pass
    /// </summary>
    /// <param name="Pass"></param>
    public void Assign(CellPass Pass)
    {
      GPSModeStore = Pass.GPSModeStore;
      //MachineID = Pass.MachineID;
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
      //writer.Write(MachineID);
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
      //MachineID = reader.ReadInt64();
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
    /// Serialises content of the cell to the writer
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
    /// Serialises content of the cell from the writer
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      GPSModeStore = reader.ReadByte();
      InternalSiteModelMachineIndex = reader.ReadShort();
      Height = reader.ReadFloat();
      Time = DateTime.FromFileTime(reader.ReadLong());
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
