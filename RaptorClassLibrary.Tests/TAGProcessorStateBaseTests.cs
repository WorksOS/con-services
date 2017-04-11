using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Tests
{
    [TestClass()]
    public class TAGProcessorStateBaseTests
    {
        [TestMethod()]
        public void Test_TAGProcessorStateBase_Creation()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state != null, "State did not construct");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetControlStateTilt()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ControlStateTilt == MachineControlStateFlags.NullGCSControlState, "Initial value incorrect");
            state.SetControlStateTilt(100);
            Assert.IsTrue(state.ControlStateTilt == 100, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetControlStateLeftLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ControlStateLeftLift == MachineControlStateFlags.NullGCSControlState, "Initial value incorrect");
            state.SetControlStateLeftLift(100);
            Assert.IsTrue(state.ControlStateLeftLift == 100, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetControlStateLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ControlStateLift == MachineControlStateFlags.NullGCSControlState, "Initial value incorrect");
            state.SetControlStateLift(100);
            Assert.IsTrue(state.ControlStateLift == 100, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetControlStateRightLift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ControlStateRightLift == MachineControlStateFlags.NullGCSControlState, "Initial value incorrect");
            state.SetControlStateRightLift(100);
            Assert.IsTrue(state.ControlStateRightLift == 100, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetControlStateSideShift()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ControlStateSideShift == MachineControlStateFlags.NullGCSControlState, "Initial value incorrect");
            state.SetControlStateSideShift(100);
            Assert.IsTrue(state.ControlStateSideShift == 100, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetAndGetMachineDirection()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            // Note: Machine direction cannot be set after a gear value is selected
            Assert.IsTrue(state.MachineDirection == MachineDirection.Unknown, "Initial value incorrect");

            state.SetMachineDirection(MachineDirection.Forward);
            Assert.IsTrue(state.MachineDirection == MachineDirection.Forward, "Value incorrect after setting");

            state.ICGear = MachineGear.Forward;

            state.SetMachineDirection(MachineDirection.Reverse);
            Assert.IsFalse(state.MachineDirection == MachineDirection.Reverse, "Value changed incorrectly after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetResearchData()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(!state.ResearchData, "Initial value incorrect");
            state.SetResearchData(true);
            Assert.IsTrue(state.ResearchData, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetUsingCCA()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(!state.UsingCCA, "Initial value incorrect");
            state.SetUsingCCA(true);
            Assert.IsTrue(state.UsingCCA, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_HaveReceivedValidTipPositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsFalse(state.HaveReceivedValidTipPositions, "Initial value incorrect");

            state.DataLeft = new XYZ(10.0, 10.0, 10.0);
            state.DataRight.X = 10.0;
            state.DataRight.Y = 10.0;

            Assert.IsFalse(state.HaveReceivedValidTipPositions, "Value incorrect after setting");

            state.DataRight.Z = 10.0;

            Assert.IsTrue(state.HaveReceivedValidTipPositions, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_HaveReceivedValidTrackPositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsFalse(state.HaveReceivedValidTrackPositions, "Initial value incorrect");

            state.DataTrackLeft = new XYZ(10.0, 10.0, 10.0);
            state.DataTrackRight.X = 10.0;
            state.DataTrackRight.Y = 10.0;
                      
            Assert.IsFalse(state.HaveReceivedValidTrackPositions, "Value incorrect after setting");
                      
            state.DataTrackRight.Z = 10.0;

            Assert.IsTrue(state.HaveReceivedValidTrackPositions, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_HaveReceivedValidWheelPositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsFalse(state.HaveReceivedValidWheelPositions, "Initial value incorrect");

            state.DataWheelLeft = new XYZ(10.0, 10.0, 10.0);
            state.DataWheelRight.X = 10.0;
            state.DataWheelRight.Y = 10.0;

            Assert.IsFalse(state.HaveReceivedValidWheelPositions, "Value incorrect after setting");

            state.DataWheelRight.Z = 10.0;

            Assert.IsTrue(state.HaveReceivedValidWheelPositions, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_HaveReceivedValidRearPositions()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsFalse(state.HaveReceivedValidRearPositions, "Initial value incorrect");

            state.DataRearLeft = new XYZ(10.0, 10.0, 10.0);
            state.DataRearRight.X = 10.0;
            state.DataRearRight.Y = 10.0;

            Assert.IsFalse(state.HaveReceivedValidRearPositions, "Value incorrect after setting");

            state.DataRearRight.Z = 10.0;

            Assert.IsTrue(state.HaveReceivedValidRearPositions, "Value incorrect after setting");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetGPSMode()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.GPSModes != null && (GPSMode)state.GPSModes.GetLatest() == CellPass.NullGPSMode, "Initial value incorrect");
            state.SetGPSMode(GPSMode.Fixed);
            Assert.IsTrue(state.GPSModes.NumAttrs == 2 && (GPSMode)state.GPSModes.GetLatest() == GPSMode.Fixed, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetOnGround()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.OnGrounds != null && (OnGroundState)state.OnGrounds.GetLatest() == OnGroundState.YesLegacy, "Initial value incorrect");
            state.SetOnGround(OnGroundState.No);
            Assert.IsTrue(state.OnGrounds.NumAttrs == 2 && (OnGroundState)state.OnGrounds.GetLatest() == OnGroundState.No, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICCCVValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICCCVValues != null && (short)state.ICCCVValues.GetLatest() == CellPass.NullCCV, "Initial value incorrect");
            state.SetICCCVValue(100);
            Assert.IsTrue(state.ICCCVValues.NumAttrs == 2 && (short)state.ICCCVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICMachineSpeedValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICMachineSpeedValues != null && (double)state.ICMachineSpeedValues.GetLatest() == Consts.NullDouble, "Initial value incorrect");
            state.SetICMachineSpeedValue(100);
            Assert.IsTrue(state.ICMachineSpeedValues.NumAttrs == 2 && (double)state.ICMachineSpeedValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICFrequency()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICFrequencys != null && (ushort)state.ICFrequencys.GetLatest() == CellPass.NullFrequency, "Initial value incorrect");
            state.SetICFrequency(100);
            Assert.IsTrue(state.ICFrequencys.NumAttrs == 2 && (ushort)state.ICFrequencys.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICAmplitude()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICAmplitudes != null && (ushort)state.ICAmplitudes.GetLatest() == CellPass.NullAmplitude, "Initial value incorrect");
            state.SetICAmplitude(100);
            Assert.IsTrue(state.ICAmplitudes.NumAttrs == 2 && (ushort)state.ICAmplitudes.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICRMVValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICRMVValues != null && (short)state.ICRMVValues.GetLatest() == CellPass.NullRMV, "Initial value incorrect");
            state.SetICRMVValue(100);
            Assert.IsTrue(state.ICRMVValues.NumAttrs == 2 && (short)state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetAgeOfCorrection()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.AgeOfCorrections != null && (byte)state.AgeOfCorrections.GetLatest() == 0, "Initial value incorrect");
            state.SetAgeOfCorrection((byte)100);
            Assert.IsTrue(state.AgeOfCorrections.NumAttrs == 2 && (byte)state.AgeOfCorrections.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetVolkelMeasRange()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.VolkelMeasureRanges != null && (int)state.VolkelMeasureRanges.GetLatest() == CellPass.NullVolkelMeasRange, "Initial value incorrect");
            state.SetVolkelMeasRange(100);
            Assert.IsTrue(state.VolkelMeasureRanges.NumAttrs == 2 && (int)state.VolkelMeasureRanges.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetVolkelMeasUtilRange()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.VolkelMeasureUtilRanges != null && (int)state.VolkelMeasureUtilRanges.GetLatest() == CellPass.NullVolkelMeasUtilRange, "Initial value incorrect");
            state.SetVolkelMeasUtilRange(100);
            Assert.IsTrue(state.VolkelMeasureUtilRanges.NumAttrs == 2 && (int)state.VolkelMeasureUtilRanges.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetMinElevMappingState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.MinElevMapping == false, "Initial value incorrect");
            state.SetMinElevMappingState(true);
            Assert.IsTrue(state.MinElevMapping == true, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetInAvoidZone_State()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.InAvoidZone == 0, "Initial value incorrect");
            state.SetInAvoidZoneState(100);
            Assert.IsTrue(state.InAvoidZone == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetPositioningTechState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.PositioningTech == PositioningTech.Unknown, "Initial value incorrect");
            state.SetPositioningTechState(PositioningTech.UTS);
            Assert.IsTrue(state.PositioningTech == PositioningTech.UTS, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetGPSAccuracyState()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.GPSAccuracy == GPSAccuracy.Unknown, "Initial value incorrect");
            state.SetGPSAccuracyState(GPSAccuracy.Fine, 1000);
            Assert.IsTrue(state.GPSAccuracy == GPSAccuracy.Fine && state.GPSAccuracyErrorLimit == 1000, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICMDPValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICMDPValues != null && (short)state.ICMDPValues.GetLatest() == CellPass.NullMDP, "Initial value incorrect");
            state.SetICMDPValue(100);
            Assert.IsTrue(state.ICMDPValues.NumAttrs == 2 && (short)state.ICMDPValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICCCAValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICRMVValues != null && (short)state.ICRMVValues.GetLatest() == CellPass.NullRMV, "Initial value incorrect");
            state.SetICRMVValue(100);
            Assert.IsTrue(state.ICRMVValues.NumAttrs == 2 && (short)state.ICRMVValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_SetICTemperatureValue()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ICTemperatureValues != null && (ushort)state.ICTemperatureValues.GetLatest() == CellPass.NullMaterialTemp, "Initial value incorrect");
            state.SetICTemperatureValue((ushort)100);
            Assert.IsTrue(state.ICTemperatureValues.NumAttrs == 2 && (ushort)state.ICTemperatureValues.GetLatest() == 100, "Initial value incorrect");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_ProcessEpochContext()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.ProcessEpochContext(), "Base function failed - it is not supposed to be implemented!");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_DoEpochStateEvent()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.DoEpochStateEvent(Types.EpochStateEvent.Unknown), "Base function failed - it is not supposed to be implemented!");
        }

        [TestMethod()]
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

            Assert.IsTrue(state.ConvertedBladePositions.Count == 4 &&
                state.ConvertedRearAxlePositions.Count == 4 &&
                state.ConvertedTrackPositions.Count == 4 &&
                state.ConvertedWheelPositions.Count == 4,
                "UTM point pairs not assigned to converted position arrays");
        }

        [TestMethod()]
        public void Test_TAGProcessorStateBase_GetLatestMachineSpeed()
        {
            TAGProcessorStateBase state = new TAGProcessorStateBase();

            Assert.IsTrue(state.GetLatestMachineSpeed() == Consts.NullDouble, "Initial value incorrect");
            state.SetICMachineSpeedValue(100);
            Assert.IsTrue(state.GetLatestMachineSpeed() == 100, "Value incorrect after setting");
        }
    }
}