using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
  public class TAGProcessorStateBaseTests
  {
    [Fact()]
    public void Test_TAGProcessorStateBase_Creation()
    {
      var state = new TAGProcessorStateBase();

      Assert.NotNull(state);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetControlStateTilt()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.ControlStateTilt, MachineControlStateFlags.NullGCSControlState);
      state.SetControlStateTilt(100);
      Assert.Equal(100, state.ControlStateTilt);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetControlStateLeftLift()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.ControlStateLeftLift, MachineControlStateFlags.NullGCSControlState);
      state.SetControlStateLeftLift(100);
      Assert.Equal(100, state.ControlStateLeftLift);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetControlStateLift()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.ControlStateLift, MachineControlStateFlags.NullGCSControlState);
      state.SetControlStateLift(100);
      Assert.Equal(100, state.ControlStateLift);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetControlStateRightLift()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.ControlStateRightLift, MachineControlStateFlags.NullGCSControlState);
      state.SetControlStateRightLift(100);
      Assert.Equal(100, state.ControlStateRightLift);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetControlStateSideShift()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.ControlStateSideShift, MachineControlStateFlags.NullGCSControlState);
      state.SetControlStateSideShift(100);
      Assert.Equal(100, state.ControlStateSideShift);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetAndGetMachineDirection()
    {
      var state = new TAGProcessorStateBase();

      // Note: Machine direction cannot be set after a gear value is selected
      Assert.Equal(MachineDirection.Unknown, state.MachineDirection);

      state.SetMachineDirection(MachineDirection.Forward);
      Assert.Equal(MachineDirection.Forward, state.MachineDirection);

      state.ICGear = MachineGear.Forward;

      state.SetMachineDirection(MachineDirection.Reverse);
      Assert.NotEqual(MachineDirection.Reverse, state.MachineDirection);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetResearchData()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.ResearchData, "Initial value incorrect");
      state.SetResearchData(true);
      Assert.True(state.ResearchData, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetUsingCCA()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.UsingCCA, "Initial value incorrect");
      state.SetUsingCCA(true);
      Assert.True(state.UsingCCA, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_HaveReceivedValidTipPositions()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.HaveReceivedValidTipPositions, "Initial value incorrect");

      state.DataLeft = new XYZ(10.0, 10.0, 10.0);
      state.DataRight.X = 10.0;
      state.DataRight.Y = 10.0;

      Assert.False(state.HaveReceivedValidTipPositions, "Value incorrect after setting");

      state.DataRight.Z = 10.0;

      Assert.True(state.HaveReceivedValidTipPositions, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_HaveReceivedValidTrackPositions()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.HaveReceivedValidTrackPositions, "Initial value incorrect");

      state.DataTrackLeft = new XYZ(10.0, 10.0, 10.0);
      state.DataTrackRight.X = 10.0;
      state.DataTrackRight.Y = 10.0;

      Assert.False(state.HaveReceivedValidTrackPositions, "Value incorrect after setting");

      state.DataTrackRight.Z = 10.0;

      Assert.True(state.HaveReceivedValidTrackPositions, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_HaveReceivedValidWheelPositions()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.HaveReceivedValidWheelPositions, "Initial value incorrect");

      state.DataWheelLeft = new XYZ(10.0, 10.0, 10.0);
      state.DataWheelRight.X = 10.0;
      state.DataWheelRight.Y = 10.0;

      Assert.False(state.HaveReceivedValidWheelPositions, "Value incorrect after setting");

      state.DataWheelRight.Z = 10.0;

      Assert.True(state.HaveReceivedValidWheelPositions, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_HaveReceivedValidRearPositions()
    {
      var state = new TAGProcessorStateBase();

      Assert.False(state.HaveReceivedValidRearPositions, "Initial value incorrect");

      state.DataRearLeft = new XYZ(10.0, 10.0, 10.0);
      state.DataRearRight.X = 10.0;
      state.DataRearRight.Y = 10.0;

      Assert.False(state.HaveReceivedValidRearPositions, "Value incorrect after setting");

      state.DataRearRight.Z = 10.0;

      Assert.True(state.HaveReceivedValidRearPositions, "Value incorrect after setting");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetGPSMode()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.GPSModes.GetLatest() == CellPassConsts.NullGPSMode, "Initial value incorrect");
      state.SetGPSMode(GPSMode.Fixed);
      Assert.True(state.GPSModes.NumAttrs == 2 && state.GPSModes.GetLatest() == GPSMode.Fixed, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetOnGround()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.OnGrounds.GetLatest() == OnGroundState.YesLegacy, "Initial value incorrect");
      state.SetOnGround(OnGroundState.No);
      Assert.True(state.OnGrounds.NumAttrs == 2 && state.OnGrounds.GetLatest() == OnGroundState.No, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICCCVValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICCCVValues.GetLatest() == CellPassConsts.NullCCV, "Initial value incorrect");
      state.SetICCCVValue(100);
      Assert.True(state.ICCCVValues.NumAttrs == 2 && state.ICCCVValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICMachineSpeedValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICMachineSpeedValues.GetLatest() == Consts.NullDouble, "Initial value incorrect");
      state.SetICMachineSpeedValue(100);
      Assert.True(state.ICMachineSpeedValues.NumAttrs == 2 && state.ICMachineSpeedValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICFrequency()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICFrequencys.GetLatest() == CellPassConsts.NullFrequency, "Initial value incorrect");
      state.SetICFrequency(100);
      Assert.True(state.ICFrequencys.NumAttrs == 2 && state.ICFrequencys.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICAmplitude()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICAmplitudes.GetLatest() == CellPassConsts.NullAmplitude, "Initial value incorrect");
      state.SetICAmplitude(100);
      Assert.True(state.ICAmplitudes.NumAttrs == 2 && state.ICAmplitudes.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICRMVValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICRMVValues.GetLatest() == CellPassConsts.NullRMV, "Initial value incorrect");
      state.SetICRMVValue(100);
      Assert.True(state.ICRMVValues.NumAttrs == 2 && state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetAgeOfCorrection()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.AgeOfCorrections.GetLatest() == 0, "Initial value incorrect");
      state.SetAgeOfCorrection(100);
      Assert.True(state.AgeOfCorrections.NumAttrs == 2 && (byte)state.AgeOfCorrections.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetVolkelMeasRange()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.VolkelMeasureRanges.GetLatest() == CellPassConsts.NullVolkelMeasRange, "Initial value incorrect");
      state.SetVolkelMeasRange(100);
      Assert.True(state.VolkelMeasureRanges.NumAttrs == 2 && state.VolkelMeasureRanges.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetVolkelMeasUtilRange()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.VolkelMeasureUtilRanges.GetLatest() == CellPassConsts.NullVolkelMeasUtilRange, "Initial value incorrect");
      state.SetVolkelMeasUtilRange(100);
      Assert.True(state.VolkelMeasureUtilRanges.NumAttrs == 2 && state.VolkelMeasureUtilRanges.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetElevationMappingModeState()
    {
      var state = new TAGProcessorStateBase();

      state.ElevationMappingMode.Should().Be(ElevationMappingMode.LatestElevation);

      state.SetElevationMappingModeState(ElevationMappingMode.MinimumElevation);
      state.ElevationMappingMode.Should().Be(ElevationMappingMode.MinimumElevation);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetInAvoidZone_State()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(0, state.InAvoidZone);
      state.SetInAvoidZoneState(100);
      Assert.Equal(100, state.InAvoidZone);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetPositioningTechState()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(PositioningTech.Unknown, state.PositioningTech);
      state.SetPositioningTechState(PositioningTech.UTS);
      Assert.Equal(PositioningTech.UTS, state.PositioningTech);
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetGPSAccuracyState()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(GPSAccuracy.Unknown, state.GPSAccuracy);
      state.SetGPSAccuracyState(GPSAccuracy.Fine, 1000);
      Assert.True(state.GPSAccuracy == GPSAccuracy.Fine && state.GPSAccuracyErrorLimit == 1000, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICMDPValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICMDPValues.GetLatest() == CellPassConsts.NullMDP, "Initial value incorrect");
      state.SetICMDPValue(100);
      Assert.True(state.ICMDPValues.NumAttrs == 2 && state.ICMDPValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICCCAValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICRMVValues.GetLatest() == CellPassConsts.NullRMV, "Initial value incorrect");
      state.SetICRMVValue(100);
      Assert.True(state.ICRMVValues.NumAttrs == 2 && state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetICTemperatureValue()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ICTemperatureValues.GetLatest() == CellPassConsts.NullMaterialTemperatureValue, "Initial value incorrect");
      state.SetICTemperatureValue(100);
      Assert.True(state.ICTemperatureValues.NumAttrs == 2 && state.ICTemperatureValues.GetLatest() == 100, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_SetStartProofingState()
    {
      var state = new TAGProcessorStateBase();

      var startProofing = "Start Proofing Run";

      Assert.Equal(string.Empty, state.StartProofing);
      state.StartProofing = startProofing;
      Assert.True(state.StartProofing == startProofing, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_StartProofingDataTime()
    {
      var state = new TAGProcessorStateBase();

      var startProofingDataTime = DateTime.Now;

      Assert.Equal(Consts.MIN_DATETIME_AS_UTC, state.StartProofingDataTime);
      state.StartProofingDataTime = startProofingDataTime;
      Assert.True(state.StartProofingDataTime == startProofingDataTime, "Initial value incorrect");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_ProcessEpochContext()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.ProcessEpochContext(), "Base function failed - it is not supposed to be implemented!");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_DoEpochStateEvent()
    {
      var state = new TAGProcessorStateBase();

      Assert.True(state.DoEpochStateEvent(TRex.TAGFiles.Types.EpochStateEvent.Unknown), "Base function failed - it is not supposed to be implemented!");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_PopulateConvertedBladeAndRearTypePositions()
    {
      var state = new TAGProcessorStateBase();

      UTMCoordPointPair pair = UTMCoordPointPair.Null;

      pair.Left.X = 10;
      pair.Right.X = 10;

      List<UTMCoordPointPair> list1 = new List<UTMCoordPointPair>() { pair, pair, pair, pair };
      List<UTMCoordPointPair> list2 = new List<UTMCoordPointPair>() { pair, pair, pair, pair };
      List<UTMCoordPointPair> list3 = new List<UTMCoordPointPair>() { pair, pair, pair, pair };
      List<UTMCoordPointPair> list4 = new List<UTMCoordPointPair>() { pair, pair, pair, pair };

      state.PopulateConvertedBladeAndRearTypePositions(list1, list2, list3, list4);

      Assert.True(state.ConvertedBladePositions.Count == 4 &&
          state.ConvertedRearAxlePositions.Count == 4 &&
          state.ConvertedTrackPositions.Count == 4 &&
          state.ConvertedWheelPositions.Count == 4,
          "UTM point pairs not assigned to converted position arrays");
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_GetLatestMachineSpeed()
    {
      var state = new TAGProcessorStateBase();

      Assert.Equal(state.GetLatestMachineSpeed(), Consts.NullDouble);
      state.SetICMachineSpeedValue(100);
      Assert.Equal(100, state.GetLatestMachineSpeed());
    }

    [Fact()]
    public void Test_TAGProcessorStateBase_MachineControlTypeEmpty()
    {
      var state = new TAGProcessorStateBase();
      state.HardwareID.Should().BeNullOrEmpty();
      state.Invoking(x => x.GetPlatformType()).Should().Throw<ArgumentException>().WithMessage("No mapping exists for this serial number");
    }


    [Fact()]
    public void Test_TAGProcessorStateBase_MachineControlTypeValid()
    {
      var state = new TAGProcessorStateBase();
      state.HardwareID = "2432J011SW";
      state.GetPlatformType().Should().Be(MachineControlPlatformType.CB460);
    }
  }
}
