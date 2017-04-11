using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Time;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CMV;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.PassCount;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Temperature;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.CoordinateSystem;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Events;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Location;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Sensors;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Telematics;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Positioning;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Proofing;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Time;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Tests
{
    public class TAGProcessorStateBase_Test : TAGProcessorStateBase
    {
        public EpochStateEvent TriggeredEpochStateEvent = EpochStateEvent.Unknown;

        public override bool DoEpochStateEvent(EpochStateEvent eventType)
        {
            TriggeredEpochStateEvent = eventType;

            return base.DoEpochStateEvent(eventType);
        }
    }

    [TestClass()]
    public class TAGValueMatcherTests
    {
        private static void InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state)
        {
            sink = new TAGProcessorStateBase_Test(); // TAGProcessorStateBase();
            state = new TAGValueMatcherState();
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ApplicationVersion()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGApplicationVersionValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("Test")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.ApplicationVersion == "Test", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_CCATargetValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGCCATargetValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.ICCCATargetValue == 100, "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_CCAValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGCCAValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((short)sink.ICCCAValues.GetLatest() == 100, "TAG value not processed as expected");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0)),
                          "Matcher process function returned false");

            Assert.IsTrue((short)sink.ICCCAValues.GetLatest() == CellPass.NullCCA, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_UsingCCA()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGUsingCCAValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0),
                                                1),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.UsingCCA, "TAG value not processed as expected");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0),
                                                0),
                          "Matcher process function returned false");

            Assert.IsFalse(sink.UsingCCA, "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_CCVValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGCCVValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((short)sink.ICCCVValues.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((short)sink.ICCCVValues.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

            Assert.IsTrue((short)sink.ICCCVValues.GetLatest() == CellPass.NullCCV, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TargetCCVValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTargetCCVValueMatcher(sink, state);

            Assert.IsTrue(sink.ICCCVTargetValue == CellPass.NullCCV, "Incorrect value before assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                               100),
                           "Matcher process function returned false");

            Assert.IsTrue(sink.ICCCVTargetValue == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MDPValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMDPValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((short)sink.ICMDPValues.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((short)sink.ICMDPValues.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

            Assert.IsTrue((short)sink.ICMDPValues.GetLatest() == CellPass.NullMDP, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TargetMDPValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTargetMDPValueMatcher(sink, state);

            Assert.IsTrue(sink.ICMDPTargetValue == CellPass.NullMDP, "Incorrect value before assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                               100),
                           "Matcher process function returned false");

            Assert.IsTrue(sink.ICMDPTargetValue == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_PassCount()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTargetPassCountValueMatcher(sink, state);

            Assert.IsTrue(sink.ICPassTargetValue == 0, "Incorrect value before assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                               100),
                           "Matcher process function returned false");

            Assert.IsTrue(sink.ICPassTargetValue == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Temperature()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTemperatureValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((ushort)sink.ICTemperatureValues.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((ushort)sink.ICTemperatureValues.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

            Assert.IsTrue((ushort)sink.ICTemperatureValues.GetLatest() == CellPass.NullMaterialTemp, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TemperatureWarningLevelMin()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTemperatureWarningLevelMinValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICTempWarningLevelMinValue == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TemperatureWarningLevelMax()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTemperatureWarningLevelMaxValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICTempWarningLevelMaxValue == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Amplitude()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGAmplitudeValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((ushort)sink.ICAmplitudes.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((ushort)sink.ICAmplitudes.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

            Assert.IsTrue((ushort)sink.ICAmplitudes.GetLatest() == CellPass.NullAmplitude, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Frequency()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGFrequencyValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((ushort)sink.ICFrequencys.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((ushort)sink.ICFrequencys.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

            Assert.IsTrue((ushort)sink.ICFrequencys.GetLatest() == CellPass.NullFrequency, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_RMVJumpthreshold()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGRMVJumpThresholdValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICRMVJumpthreshold == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_RMV()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGRMVValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((short)sink.ICRMVValues.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((short)sink.ICRMVValues.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

            Assert.IsTrue((short)sink.ICRMVValues.GetLatest() == CellPass.NullRMV, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ICModeFlags()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGICModeFlagsValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              8),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICMode == 8, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_LayerID()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLayerIDValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              8),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICLayerIDValue == 8, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TargetLiftThickness()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTargetLiftThicknessValueMatcher(sink, state);

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t16bitUInt, 0),
                                                              1000), // supplied as millimeters
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICTargetLiftThickness == 1.0, "Incorrect value after assignment"); // Presented as meters
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_VolkelMeasurementRangeUtil()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGVolkelMeasurementRangeUtilValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.IsFalse(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                               100),
                           "Offset accepted before absolute");

            // Test value sets correctly
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue((int)sink.VolkelMeasureUtilRanges.GetLatest() == 1000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0),
                                                100),
                          "Matcher process function returned false");

            Assert.IsTrue((int)sink.VolkelMeasureUtilRanges.GetLatest() == 1100, "Incorrect value after assignment");

            // Test value ussets correctly on an empty value
            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitInt, 0)),
                          "Matcher process function returned false");

            Assert.IsFalse(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

            Assert.IsTrue((int)sink.VolkelMeasureUtilRanges.GetLatest() == CellPass.NullVolkelMeasUtilRange, "TAG value not proceed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ControlStateLeftLift()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGControlStateLeftLiftValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ControlStateLeftLift == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ControlStateRightLift()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGControlStateRightLiftValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ControlStateRightLift == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ControlStateLift()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            TAGControlStateLiftValueMatcher matcher = new TAGControlStateLiftValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ControlStateLift == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ControlStateTilt()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGControlStateTiltValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ControlStateTilt == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ControlStateSideShift()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGControlStateSideShiftValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t12bitUInt, 0),
                                                              1000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ControlStateSideShift == 1000, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_CoordinateSystemType()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGCoordinateSystemTypeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              0), // Unknown CS
                          "Matcher process function returned false");
            Assert.IsTrue(sink.CSType == CoordinateSystemType.NoCoordSystem, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              1), // Project calibration (CSIB)
                          "Matcher process function returned false");
            Assert.IsTrue(sink.CSType == CoordinateSystemType.CSIB, "Incorrect value after assignment");
            Assert.IsTrue(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              2), // Automatic Coordinate System (ACS)
                          "Matcher process function returned false");
            Assert.IsTrue(sink.CSType == CoordinateSystemType.ACS, "Incorrect value after assignment");
            Assert.IsFalse(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              3),  // Invalid
                          "Matcher process function returned false");
        }


        [TestMethod()]
        public void Test_TAGValueMatcher_UTMZone()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGUTMZoneValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0),
                                                              50),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.UTMZone == 50, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MachineShutdown()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMachineShutdownValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent == EpochStateEvent.MachineShutdown,
                          "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MachineStartup()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMachineStartupValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent == EpochStateEvent.MachineStartup,
                          "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MapReset()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMapResetValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent == EpochStateEvent.MachineMapReset,
                          "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_UTSMode()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGUTSModeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(((TAGProcessorStateBase_Test)sink).TriggeredEpochStateEvent == EpochStateEvent.MachineInUTSMode,
                          "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Height()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGHeightValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessDoubleValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0), 100.0),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.LLHHeight == 100.0, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Longitude()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLongitudeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessDoubleValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0), 100.0),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.LLHLon == 100.0, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Latitude()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLatitudeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessDoubleValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0), 100.0),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.LLHLat == 100.0, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_CompactorSensorType()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            TAGCompactorSensorTypeValueMatcher matcher = new TAGCompactorSensorTypeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              0), // No sensor
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICSensorType == CompactionSensorType.NoSensor, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              1), // MC 024
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICSensorType == CompactionSensorType.MC024, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              2), // Volkel
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICSensorType == CompactionSensorType.Volkel, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              3), // CAT factory fit
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICSensorType == CompactionSensorType.CATFactoryFitSensor, "Incorrect value after assignment");

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              4),  // Invalid
                          "Matcher process function returned false");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_VolkelMeasurementRange()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGVolkelMeasurementRangeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue((int)sink.VolkelMeasureRanges.GetLatest() == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_RadioSerial()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGRadioSerialValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("Test")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.RadioSerial == "Test", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_RadioType()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGRadioTypeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("Test")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.RadioType == "Test", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_BladeOnGround()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGBladeOnGroundValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  0), // No
                          "Matcher process function returned false");

            Assert.IsTrue((OnGroundState)sink.OnGrounds.GetLatest() == OnGroundState.No, "TAG value not processed as expected");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  5), //Yes, Remote Switch
                          "Matcher process function returned false");

            Assert.IsTrue((OnGroundState)sink.OnGrounds.GetLatest() == OnGroundState.YesRemoteSwitch, "TAG value not processed as expected");

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  7), // Invalid
                          "Matcher process function returned false");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_OnGround()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGOnGroundValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  0), // No
                          "Matcher process function returned false");

            Assert.IsTrue((OnGroundState)sink.OnGrounds.GetLatest() == OnGroundState.No, "TAG value not processed as expected");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  1), //Yes, Legacy
                          "Matcher process function returned false");

            Assert.IsTrue((OnGroundState)sink.OnGrounds.GetLatest() == OnGroundState.YesLegacy, "TAG value not processed as expected");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                  2), // Invalid -> unknown
                          "Matcher process function returned false");
            Assert.IsTrue((OnGroundState)sink.OnGrounds.GetLatest() == OnGroundState.Unknown, "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Design()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGDesignValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnicodeStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  "Test"),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.Design == "Test", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Gear()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGGearValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              3), // Sensor failed
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ICGear == MachineGear.SensorFailedDeprecated, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MachineID()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMachineIDValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("Test")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.MachineID == "Test", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MachineSpeed()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMachineSpeedValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessDoubleValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0), 100.0),
                          "Matcher process function returned false");
            Assert.IsTrue((double)sink.ICMachineSpeedValues.GetLatest() == 100.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenMachineSpeed, "HaveSeenMachineSpeed not set");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MachineType()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMachineTypeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t8bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MachineType == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_MinElevMapping()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGMinElevMappingValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              0),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MinElevMapping == false, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              1),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MinElevMapping == true, "Incorrect value after assignment");

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              2),
                          "Matcher process function returned false");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Sequence()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGSequenceValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t32bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.Sequence == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_WheelWidth()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGWheelWidthValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessDoubleValue(new TAGDictionaryItem("", Types.TAGDataType.tIEEEDouble, 0),
                                                     1.0),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MachineWheelWidth == 1.0, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_LeftRightBlade()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLeftRightBladeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.Side == TAGValueSide.Left, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.Side == TAGValueSide.Right, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_LeftRightTrack()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLeftRightTrackValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftTrackTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.TrackSide == TAGValueSide.Left, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightTrackTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.TrackSide == TAGValueSide.Right, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_LeftRightWheel()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLeftRightWheelValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftWheelTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.WheelSide == TAGValueSide.Left, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightWheelTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.WheelSide == TAGValueSide.Right, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_LeftRightRear()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGLeftRightRearValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftRearTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.RearSide == TAGValueSide.Left, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightRearTag, Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.RearSide == TAGValueSide.Right, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_BladeOrdinateValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGBladeOrdinateValueMatcher(sink, state);

            state.Side = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, Types.TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, Types.TAGDataType.t32bitInt, 0);

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataLeft.X == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileNorthingTag; 
            offsetItem.Name = TAGValueNames.kTagFileNorthingTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataLeft.Y == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileElevationTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataLeft.Z == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataLeft.Z == 1.5, "Incorrect value after assignment");

            // Test an empty value clears the absolute value seen flag

            Assert.IsTrue(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.IsFalse(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_TrackOrdinateValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTrackOrdinateValueMatcher(sink, state);

            state.TrackSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, Types.TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, Types.TAGDataType.t32bitInt, 0);

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataTrackLeft.X == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataTrackLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileNorthingTrackTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingTrackTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataTrackLeft.Y == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataTrackLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileElevationTrackTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationTrackTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataTrackLeft.Z == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataTrackLeft.Z == 1.5, "Incorrect value after assignment");

            // Test an empty value clears the absolute value seen flag

            Assert.IsTrue(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.IsFalse(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_WheelOrdinateValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGWheelOrdinateValueMatcher(sink, state);

            state.WheelSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, Types.TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, Types.TAGDataType.t32bitInt, 0);

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataWheelLeft.X == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataWheelLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileNorthingWheelTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingWheelTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataWheelLeft.Y == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataWheelLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileElevationWheelTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationWheelTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataWheelLeft.Z == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataWheelLeft.Z == 1.5, "Incorrect value after assignment");

            // Test an empty value clears the absolute value seen flag

            Assert.IsTrue(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.IsFalse(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_RearOrdinateValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGRearOrdinateValueMatcher(sink, state);

            state.RearSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, Types.TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem = new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, Types.TAGDataType.t32bitInt, 0);

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataRearLeft.X == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataRearLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileNorthingRearTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingRearTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataRearLeft.Y == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataRearLeft.X == 1.5, "Incorrect value after assignment");

            absoluteItem.Name = TAGValueNames.kTagFileElevationRearTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationRearTag;

            Assert.IsTrue(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.IsTrue(sink.DataRearLeft.Z == 1.0, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.IsTrue(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.IsTrue(sink.DataRearLeft.Z == 1.5, "Incorrect value after assignment");

            // Test an empty value clears the absolute value seen flag

            Assert.IsTrue(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.IsFalse(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_GPSAccuracy()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGGPSAccuracyValueMatcher(sink, state);

            // Set GPS accuracy word to be Medium accuracy with a 100mm error limit
            ushort GPSAccuracyWord = (0x1 << 14) | 100;

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t16bitUInt, 0),
                                                              GPSAccuracyWord),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.GPSAccuracy == GPSAccuracy.Medium && sink.GPSAccuracyErrorLimit == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_GPSBasePosition()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGGPSBasePositionValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tEmptyType, 0)),
                          "Matcher process function returned false");
            Assert.IsTrue(state.GPSBasePositionReportingHaveStarted, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_GPSMode()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGGPSModeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              2), // Float RTK
                          "Matcher process function returned false");
            Assert.IsTrue((GPSMode)sink.GPSModes.GetLatest() == GPSMode.Float, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ValidPosition()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGValidPositionValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              100),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ValidPosition == 100, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_EndProofingTimeValue()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGEndProofingTimeValueMatcher(sink, state);

            // Test the Time aspect
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingTimeTag, Types.TAGDataType.t32bitUInt, 0),
                                                              10000000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.StartProofingTime == 10000000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAProofingRunTimeValue, "Incorrect value after assignment");

            // Test the Week aspect
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingWeekTag, Types.TAGDataType.t16bitUInt, 0),
                                                              10000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.StartProofingWeek == 10000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAProofingRunWeekValue, "Incorrect value after assignment");

            // Test the actual start proofing run time value is calculated correctly
            Assert.IsTrue(sink.StartProofingTime == 10000000 &&
                sink.StartProofingWeek == 10000 &&
                GPS.GPSOriginTimeToDateTime(sink.StartProofingWeek, sink.StartProofingTime) == sink.StartProofingDataTime,
                "Start proofing data time not as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_EndProofing()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGEndProofingValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("TestEnd")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.EndProofingName == "TestEnd", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_StartProofing()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGStartProofingValueMatcher(sink, state);

            // Set time and week values
            sink.GPSWeekTime = 10000000;
            state.HaveSeenATimeValue = true;
            sink.GPSWeekNumber = 10000;
            state.HaveSeenAWeekValue = true;

            sink.DataTime = GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime = 10000000);

            Assert.IsTrue(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0),
                                                  ASCIIEncoding.ASCII.GetBytes("TestStart")),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.StartProofing == "TestStart", "TAG value not processed as expected");

            // Test the start proofing data time is calculated correctly
            Assert.IsTrue(sink.StartProofingDataTime == sink.DataTime, "Start proofing time not set to data time");

            Assert.IsTrue(matcher.ProcessEmptyValue(new TAGDictionaryItem("", Types.TAGDataType.tANSIString, 0)),
                          "Matcher process function returned false");

            Assert.IsTrue(sink.StartProofing == "", "TAG value not processed as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Time()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGTimeValueMatcher(sink, state);

            // Test the Time aspect
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, Types.TAGDataType.t32bitUInt, 0),
                                                              10000000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.GPSWeekTime == 10000000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenATimeValue, "Incorrect value after assignment");

            // Test the absolute Week aspect
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem(TAGValueNames.kTagFileWeekTag, Types.TAGDataType.t16bitUInt, 0),
                                                              10000),
                          "Matcher process function returned false");
            Assert.IsTrue(sink.GPSWeekNumber == 10000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAWeekValue, "Incorrect value after assignment");

            // Test the epoch data time is calculated correctly
            Assert.IsTrue(sink.GPSWeekTime == 10000000 &&
                sink.GPSWeekNumber == 10000 &&
                GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
                "Absolute epoch GPS data time not as expected");

            // Test the incremental time aspect
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, Types.TAGDataType.t4bitUInt, 0),
                                                              10), // Adds 10, 1/10 of a second intervals, for an increment of 1000ms
                          "Matcher process function returned false");
            Assert.IsTrue(sink.GPSWeekTime == 10001000, "Incorrect value after assignment");
            Assert.IsTrue(state.HaveSeenAWeekValue, "Incorrect value after assignment");

            // Test the epoch data time is calculated correctly
            Assert.IsTrue(sink.GPSWeekTime == 10001000 &&
                sink.GPSWeekNumber == 10000 &&
                GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
                "Incremented epoch GPS data time not as expected");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_AgeOfCOrrections()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGAgeValueMatcher(sink, state);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0),
                                                              10),
                          "Matcher process function returned false");
            Assert.IsTrue((byte)sink.AgeOfCorrections.GetLatest() == 10, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_3DSonic()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAG3DSonicValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0);

            for (uint i = 0; i < 3; i++)
            {
                Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, i),
                              "Matcher process function returned false");
                Assert.IsTrue(sink.ICSonic3D == i, "Incorrect value after assignment");
            }

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(dictItem, 3),
                          "Matcher process function returned false");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_Direction()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGDirectionValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0);

            // Note: Machien direction values from the machine are 1-based
            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, 1), // Forwards
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MachineDirection == MachineDirection.Forward, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, 2), // Reverse
                          "Matcher process function returned false");
            Assert.IsTrue(sink.MachineDirection == MachineDirection.Reverse, "Incorrect value after assignment");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_AvoidanceZone()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGInAvoidanceZoneValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0);

            for (uint i = 0; i < 4; i++)
            {
                Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, i),
                              "Matcher process function returned false");
                Assert.IsTrue(sink.InAvoidZone == i, "Incorrect value after assignment");
            }

            Assert.IsFalse(matcher.ProcessUnsignedIntegerValue(dictItem, 5),
                          "Matcher process function returned false");
        }

        [TestMethod()]
        public void Test_TAGValueMatcher_ResearchData()
        {
            TAGProcessorStateBase sink;
            TAGValueMatcherState state;

            InitStateAndSink(out sink, out state);
            var matcher = new TAGResearchDataValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", Types.TAGDataType.t4bitUInt, 0);

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, 0), // False
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ResearchData == false, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, 1), // False
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ResearchData == true, "Incorrect value after assignment");

            Assert.IsTrue(matcher.ProcessUnsignedIntegerValue(dictItem, 2), // Also true...
                          "Matcher process function returned false");
            Assert.IsTrue(sink.ResearchData == true, "Incorrect value after assignment");
        }
    }
}