using System.Collections.Generic;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Geometry;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests
{
        public class TAGProcessorStateBaseTests
    {
        [Fact()]
        public void Test_TAGProcessorStateBase_Creation()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.NotNull(state);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetControlStateTilt()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.ControlStateTilt, MachineControlStateFlags.NullGCSControlState);
            state.SetControlStateTilt(100);
            Assert.Equal(100, state.ControlStateTilt);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetControlStateLeftLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.ControlStateLeftLift, MachineControlStateFlags.NullGCSControlState);
            state.SetControlStateLeftLift(100);
            Assert.Equal(100, state.ControlStateLeftLift);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetControlStateLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.ControlStateLift, MachineControlStateFlags.NullGCSControlState);
            state.SetControlStateLift(100);
            Assert.Equal(100, state.ControlStateLift);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetControlStateRightLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.ControlStateRightLift, MachineControlStateFlags.NullGCSControlState);
            state.SetControlStateRightLift(100);
            Assert.Equal(100, state.ControlStateRightLift);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetControlStateSideShift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.ControlStateSideShift, MachineControlStateFlags.NullGCSControlState);
            state.SetControlStateSideShift(100);
            Assert.Equal(100, state.ControlStateSideShift);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetAndGetMachineDirection()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.False(state.ResearchData, "Initial value incorrect");
            state.SetResearchData(true);
            Assert.True(state.ResearchData, "Value incorrect after setting");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetUsingCCA()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.False(state.UsingCCA, "Initial value incorrect");
            state.SetUsingCCA(true);
            Assert.True(state.UsingCCA, "Value incorrect after setting");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_HaveReceivedValidTipPositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.GPSModes != null && (GPSMode)state.GPSModes.GetLatest() == CellPassConsts.NullGPSMode, "Initial value incorrect");
            state.SetGPSMode(GPSMode.Fixed);
            Assert.True(state.GPSModes.NumAttrs == 2 && (GPSMode)state.GPSModes.GetLatest() == GPSMode.Fixed, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetOnGround()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.OnGrounds != null && (OnGroundState)state.OnGrounds.GetLatest() == OnGroundState.YesLegacy, "Initial value incorrect");
            state.SetOnGround(OnGroundState.No);
            Assert.True(state.OnGrounds.NumAttrs == 2 && (OnGroundState)state.OnGrounds.GetLatest() == OnGroundState.No, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICCCVValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICCCVValues != null && (short)state.ICCCVValues.GetLatest() == CellPassConsts.NullCCV, "Initial value incorrect");
            state.SetICCCVValue(100);
            Assert.True(state.ICCCVValues.NumAttrs == 2 && (short)state.ICCCVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICMachineSpeedValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICMachineSpeedValues != null && (double)state.ICMachineSpeedValues.GetLatest() == Consts.NullDouble, "Initial value incorrect");
            state.SetICMachineSpeedValue(100);
            Assert.True(state.ICMachineSpeedValues.NumAttrs == 2 && (double)state.ICMachineSpeedValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICFrequency()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICFrequencys != null && (ushort)state.ICFrequencys.GetLatest() == CellPassConsts.NullFrequency, "Initial value incorrect");
            state.SetICFrequency(100);
            Assert.True(state.ICFrequencys.NumAttrs == 2 && (ushort)state.ICFrequencys.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICAmplitude()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICAmplitudes != null && (ushort)state.ICAmplitudes.GetLatest() == CellPassConsts.NullAmplitude, "Initial value incorrect");
            state.SetICAmplitude(100);
            Assert.True(state.ICAmplitudes.NumAttrs == 2 && (ushort)state.ICAmplitudes.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICRMVValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICRMVValues != null && (short)state.ICRMVValues.GetLatest() == CellPassConsts.NullRMV, "Initial value incorrect");
            state.SetICRMVValue(100);
            Assert.True(state.ICRMVValues.NumAttrs == 2 && (short)state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetAgeOfCorrection()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.AgeOfCorrections != null && (byte)state.AgeOfCorrections.GetLatest() == 0, "Initial value incorrect");
            state.SetAgeOfCorrection((byte)100);
            Assert.True(state.AgeOfCorrections.NumAttrs == 2 && (byte)state.AgeOfCorrections.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetVolkelMeasRange()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.VolkelMeasureRanges != null && (int)state.VolkelMeasureRanges.GetLatest() == CellPassConsts.NullVolkelMeasRange, "Initial value incorrect");
            state.SetVolkelMeasRange(100);
            Assert.True(state.VolkelMeasureRanges.NumAttrs == 2 && (int)state.VolkelMeasureRanges.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetVolkelMeasUtilRange()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.VolkelMeasureUtilRanges != null && (int)state.VolkelMeasureUtilRanges.GetLatest() == CellPassConsts.NullVolkelMeasUtilRange, "Initial value incorrect");
            state.SetVolkelMeasUtilRange(100);
            Assert.True(state.VolkelMeasureUtilRanges.NumAttrs == 2 && (int)state.VolkelMeasureUtilRanges.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetMinElevMappingState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.False(state.MinElevMapping);
            state.SetMinElevMappingState(true);
            Assert.True(state.MinElevMapping);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetInAvoidZone_State()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(0, state.InAvoidZone);
            state.SetInAvoidZoneState(100);
            Assert.Equal(100, state.InAvoidZone);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetPositioningTechState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(PositioningTech.Unknown, state.PositioningTech);
            state.SetPositioningTechState(PositioningTech.UTS);
            Assert.Equal(PositioningTech.UTS, state.PositioningTech);
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetGPSAccuracyState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(GPSAccuracy.Unknown, state.GPSAccuracy);
            state.SetGPSAccuracyState(GPSAccuracy.Fine, 1000);
            Assert.True(state.GPSAccuracy == GPSAccuracy.Fine && state.GPSAccuracyErrorLimit == 1000, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICMDPValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICMDPValues != null && (short)state.ICMDPValues.GetLatest() == CellPassConsts.NullMDP, "Initial value incorrect");
            state.SetICMDPValue(100);
            Assert.True(state.ICMDPValues.NumAttrs == 2 && (short)state.ICMDPValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICCCAValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICRMVValues != null && (short)state.ICRMVValues.GetLatest() == CellPassConsts.NullRMV, "Initial value incorrect");
            state.SetICRMVValue(100);
            Assert.True(state.ICRMVValues.NumAttrs == 2 && (short)state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_SetICTemperatureValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ICTemperatureValues != null && (ushort)state.ICTemperatureValues.GetLatest() == CellPassConsts.NullMaterialTemperatureValue, "Initial value incorrect");
            state.SetICTemperatureValue((ushort)100);
            Assert.True(state.ICTemperatureValues.NumAttrs == 2 && (ushort)state.ICTemperatureValues.GetLatest() == 100, "Initial value incorrect");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_ProcessEpochContext()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.ProcessEpochContext(), "Base function failed - it is not supposed to be implemented!");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_DoEpochStateEvent()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.True(state.DoEpochStateEvent(TRex.TAGFiles.Types.EpochStateEvent.Unknown), "Base function failed - it is not supposed to be implemented!");
        }

        [Fact()]
        public void Test_TAGProcessorStateBase_PopulateConvertedBladeAndRearTypePositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

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
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.Equal(state.GetLatestMachineSpeed(), Consts.NullDouble);
            state.SetICMachineSpeedValue(100);
            Assert.Equal(100, state.GetLatestMachineSpeed());
        }
    }
}
