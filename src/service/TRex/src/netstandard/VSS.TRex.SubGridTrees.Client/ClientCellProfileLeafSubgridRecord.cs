using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public struct ClientCellProfileLeafSubgridRecord : IBinaryReaderWriter
  {
       public float CellXOffset;
       public float CellYOffset;
       public DateTime LastPassTime; 
       public int PassCount;
       public int LastPassValidRadioLatency;

    /*   EventDesignNameID    : TICDesignNameID;
       MachineID            : TICMachineID;
       MachineSpeed         : TICMachineSpeed;
       LastPassValidGPSMode : TICGPSMode;
       GPSTolerance         : TICGPSTolerance;
       GPSAccuracy          : TICGPSAccuracy;
       TargetPassCount      : TICPassCountValue;
       TotalWholePasses     : Integer;
       TotalHalfPasses      : Integer;
       LayersCount          : Integer;
       LastPassValidCCV     : TICCCVValue;
       TargetCCV            : TICCCVValue;
       LastPassValidMDP     : TICMDPValue;
       TargetMDP            : TICMDPValue;
       LastPassValidRMV     : TICRMVValue;
       LastPassValidFreq    : TICVibrationFrequency;
       LastPassValidAmp     : TICVibrationAmplitude;
       TargetThickness      : TICLiftThickness;
       EventMachineGear     : TICMachineGear;
       EventVibrationState  : TICVibrationState;
       LastPassValidTemperature : TICMaterialTemperature;
*/
       public float Height;
    /*       HalfPass             : Boolean;
           CCVChange            : Single;
           LastPassValidCCA     : TICCCAValue;
           TargetCCA            : TICCCAMinPassesValue;
        */


    public void Clear()
    {
      CellXOffset = 0;
      CellYOffset = 0;
      LastPassTime = DateTime.MinValue;
      PassCount = 0;
   //   LastPassValidRadioLatency := kICNullRadioLatency;
   //   EventDesignNameID := kNoDesignNameID;
   //   MachineID := kICMachineIDNullValue;
   //   MachineSpeed := kICNullMachineSpeed;
   //   LastPassValidGPSMode := kICNUllGPSModeValue;
   //   GPSTolerance := kICNullGPSToleranceValue;
   //   GPSAccuracy := TICGPSAccuracy.gpsaUnknown;
   //   TargetPassCount := kICNullPassCountValue;
   //   TotalHalfPasses := 0;
   //   TotalWholePasses := 0;
   //   LayersCount := 0;
   //   LastPassValidCCV := kICNullCCVValue;
   //   TargetCCV := kICNullCCVValue;
   //   LastPassValidMDP := kICNullMDPValue;
   //   TargetMDP := kICNullMDPValue;
   //   LastPassValidRMV := kICNullRMVValue;
   //   LastPassValidFreq := kICNullFrequencyValue;
   //   LastPassValidAmp := kICNullAmplitudeValue;
   //   TargetThickness := kICNullOverridingTargetLiftThicknessValue;
   //   EventMachineGear := TICMachineGear.mgNull;
   //   EventVibrationState := TICVibrationState.vsInvalid;
   //   LastPassValidTemperature := kICNullMaterialTempValue;
      Height = Consts.NullHeight;
   //   HalfPass := False;
   //   CCVChange := 0;
   //   LastPassValidCCA := kICNullCCA;
   //   TargetCCA := kICNullCCATarget;
    }

    public void Read(BinaryReader reader)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
  }
}
