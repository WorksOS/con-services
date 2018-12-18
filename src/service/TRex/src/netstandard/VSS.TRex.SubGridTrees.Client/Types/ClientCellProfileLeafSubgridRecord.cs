using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client.Types
{
  public struct ClientCellProfileLeafSubgridRecord : IBinaryReaderWriter
  {
    public float CellXOffset{get; set;}
    public float CellYOffset{get; set;}
    public DateTime LastPassTime{get; set;}
    public int PassCount{get; set;}
    public int LastPassValidRadioLatency{get; set;}

    public int EventDesignNameID{get; set;}

    public short InternalSiteModelMachineIndex{get; set;}

    public ushort MachineSpeed{get; set;}

    public GPSMode LastPassValidGPSMode{get; set;}

    public ushort GPSTolerance{get; set;}

    public GPSAccuracy GPSAccuracy{get; set;}

    public int TargetPassCount{get; set;}
//    public int TotalWholePasses{get; set;}
//    public int TotalHalfPasses{get; set;}
    public int LayersCount{get; set;}
    public short LastPassValidCCV{get; set;}
    public short TargetCCV{get; set;}
    public short LastPassValidMDP{get; set;}
    public short TargetMDP{get; set;}

    public short LastPassValidRMV{get; set;}

    public ushort LastPassValidFreq{get; set;}
    public ushort LastPassValidAmp{get; set;}

    public float TargetThickness{get; set;}

    public MachineGear EventMachineGear{get; set;}

    public VibrationState EventVibrationState{get; set;}

    public ushort LastPassValidTemperature{get; set;}

    public float Height{get; set;}

    public bool HalfPass{get; set;}
    public float CCVChange{get; set;}

    public byte LastPassValidCCA{get; set;}
    public byte TargetCCA{get; set;}


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

    public static ClientCellProfileLeafSubgridRecord Null()
    {
      var record = new ClientCellProfileLeafSubgridRecord();
      record.Clear();
      return record;
    }

    public void Read(BinaryReader reader)
    {
      CellXOffset = reader.ReadSingle();
      CellYOffset = reader.ReadSingle();
      LastPassTime = DateTime.FromBinary(reader.ReadInt64());
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
      writer.Write(LastPassTime.ToBinary());
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
  }
}
