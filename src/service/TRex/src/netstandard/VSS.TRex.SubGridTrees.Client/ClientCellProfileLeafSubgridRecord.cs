using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public struct ClientCellProfileLeafSubgridRecord : IBinaryReaderWriter, IEquatable<ClientCellProfileLeafSubgridRecord>
  {
    public float CellXOffset;
    public float CellYOffset;
    public DateTime LastPassTime;
    public int PassCount;
    public int LastPassValidRadioLatency;

    public int EventDesignNameID;

    public short InternalSiteModelMachineIndex;

    public ushort MachineSpeed;

    public GPSMode LastPassValidGPSMode;

    public ushort GPSTolerance;

    public GPSAccuracy GPSAccuracy;

    public int TargetPassCount;
//    public int TotalWholePasses;
//    public int TotalHalfPasses;
    public int LayersCount;
    public short LastPassValidCCV;
    public short TargetCCV;
    public short LastPassValidMDP;
    public short TargetMDP;

    public short LastPassValidRMV;

    public ushort LastPassValidFreq;
    public ushort LastPassValidAmp;

    public float TargetThickness;

    public MachineGear EventMachineGear;

    public VibrationState EventVibrationState;

    public ushort LastPassValidTemperature;

    public float Height;

    public bool HalfPass;
    public float CCVChange;

    public byte LastPassValidCCA;
    public byte TargetCCA;


    public void Clear()
    {
      CellXOffset = 0;
      CellYOffset = 0;
      LastPassTime = DateTime.MinValue;
      PassCount = 0;
      LastPassValidRadioLatency = CellPassConsts.NullRadioLatency;
      EventDesignNameID = Consts.kNoDesignNameID;
      InternalSiteModelMachineIndex = -1;
      MachineSpeed = CellPassConsts.NullMachineSpeed;
      LastPassValidGPSMode = CellPassConsts.NullGPSMode;
      GPSTolerance = CellPassConsts.NullGPSTolerance;
      GPSAccuracy = GPSAccuracy.Unknown;
      TargetPassCount = CellPassConsts.NullPassCountValue;
//      TotalHalfPasses = 0;
//      TotalWholePasses = 0;
      LayersCount = 0;
      LastPassValidCCV = CellPassConsts.NullCCV;
      TargetCCV = CellPassConsts.NullCCV;
      LastPassValidMDP = CellPassConsts.NullMDP;
      TargetMDP = CellPassConsts.NullMDP;
      LastPassValidRMV = CellPassConsts.NullRMV;
      LastPassValidFreq = CellPassConsts.NullFrequency;
      LastPassValidAmp = CellPassConsts.NullAmplitude;
      TargetThickness = CellPassConsts.NullOverridingTargetLiftThicknessValue;
      EventMachineGear = MachineGear.Null;
      EventVibrationState = VibrationState.Invalid;
      LastPassValidTemperature = CellPassConsts.NullMaterialTemperatureValue;
      Height = Consts.NullHeight;
      HalfPass = false;
      CCVChange = 0;
      LastPassValidCCA = CellPassConsts.NullCCA;
      TargetCCA = CellTargets.NullCCATarget;
    }

    public void Read(BinaryReader reader)
    {
      CellXOffset = reader.ReadSingle();
      CellYOffset = reader.ReadSingle();
      LastPassTime = new DateTime(reader.ReadInt64());
      PassCount = reader.ReadInt32(); // Todo: Is this too big?
      InternalSiteModelMachineIndex = reader.ReadInt16();
      LastPassValidRadioLatency = reader.ReadInt32();
      EventDesignNameID = reader.ReadInt32();
      MachineSpeed = reader.ReadUInt16();
      LastPassValidGPSMode = (GPSMode) reader.ReadByte();
      GPSTolerance = reader.ReadUInt16();
      GPSAccuracy = (GPSAccuracy) reader.ReadByte();
      TargetPassCount = reader.ReadInt32();
//      TotalHalfPasses = reader.ReadInt32();
//      TotalWholePasses = reader.ReadInt32();
      LayersCount = reader.ReadInt32();
      LastPassValidCCV = reader.ReadInt16();
      TargetCCV = reader.ReadInt16();
      LastPassValidMDP = reader.ReadInt16();
      TargetMDP = reader.ReadInt16();
      LastPassValidRMV = reader.ReadInt16();
      LastPassValidFreq = reader.ReadUInt16();
      LastPassValidAmp = reader.ReadUInt16();
      TargetThickness = reader.ReadSingle();
      EventMachineGear = (MachineGear) reader.ReadByte();
      EventVibrationState = (VibrationState) reader.ReadByte();
      LastPassValidTemperature = reader.ReadUInt16();
      Height = reader.ReadSingle();
      HalfPass = reader.ReadBoolean();
      CCVChange = reader.ReadSingle();
      LastPassValidCCA = reader.ReadByte();
      TargetCCA = reader.ReadByte();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(CellXOffset);
      writer.Write(CellYOffset);
      writer.Write(LastPassTime.Ticks);
      writer.Write(PassCount);
      writer.Write(LastPassValidRadioLatency);
      writer.Write(EventDesignNameID);
      writer.Write(InternalSiteModelMachineIndex);
      writer.Write(MachineSpeed);
      writer.Write((byte) LastPassValidGPSMode);
      writer.Write(GPSTolerance);
      writer.Write((byte) GPSAccuracy);
      writer.Write(TargetPassCount);
//      writer.Write(TotalHalfPasses);
//      writer.Write(TotalWholePasses);
      writer.Write(LayersCount);
      writer.Write(LastPassValidCCV);
      writer.Write(TargetCCV);
      writer.Write(LastPassValidMDP);
      writer.Write(TargetMDP);
      writer.Write(LastPassValidRMV);
      writer.Write(LastPassValidFreq);
      writer.Write(LastPassValidAmp);
      writer.Write(TargetThickness);
      writer.Write((byte) EventMachineGear);
      writer.Write((byte) EventVibrationState);
      writer.Write(LastPassValidTemperature);
      writer.Write(Height);
      writer.Write(HalfPass);
      writer.Write(CCVChange);
      writer.Write(LastPassValidCCA);
      writer.Write(TargetCCA);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    public bool Equals(ClientCellProfileLeafSubgridRecord other)
    {
      return CellXOffset.Equals(other.CellXOffset) &&
             CellYOffset.Equals(other.CellYOffset) &&
             LastPassTime.Equals(other.LastPassTime) &&
             PassCount == other.PassCount &&
             LastPassValidRadioLatency == other.LastPassValidRadioLatency &&
             EventDesignNameID == other.EventDesignNameID &&
             InternalSiteModelMachineIndex == other.InternalSiteModelMachineIndex &&
             MachineSpeed == other.MachineSpeed &&
             LastPassValidGPSMode == other.LastPassValidGPSMode &&
             GPSTolerance == other.GPSTolerance &&
             GPSAccuracy == other.GPSAccuracy &&
             TargetPassCount == other.TargetPassCount &&
//             TotalWholePasses == other.TotalWholePasses &&
//             TotalHalfPasses == other.TotalHalfPasses &&
             LayersCount == other.LayersCount &&
             LastPassValidCCV == other.LastPassValidCCV &&
             TargetCCV == other.TargetCCV &&
             LastPassValidMDP == other.LastPassValidMDP &&
             TargetMDP == other.TargetMDP &&
             LastPassValidRMV == other.LastPassValidRMV &&
             LastPassValidFreq == other.LastPassValidFreq &&
             LastPassValidAmp == other.LastPassValidAmp &&
             TargetThickness.Equals(other.TargetThickness) &&
             EventMachineGear == other.EventMachineGear &&
             EventVibrationState == other.EventVibrationState &&
             LastPassValidTemperature == other.LastPassValidTemperature &&
             Height.Equals(other.Height) && HalfPass == other.HalfPass &&
             CCVChange.Equals(other.CCVChange) &&
             LastPassValidCCA == other.LastPassValidCCA &&
             TargetCCA == other.TargetCCA;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ClientCellProfileLeafSubgridRecord other && Equals(other);
    }

    public override int GetHashCode() => base.GetHashCode(); 
  }
}
