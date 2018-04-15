using System.Text;
using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CMV;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.PassCount;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Temperature;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.CoordinateSystem;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Events;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Location;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Sensors;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Telematics;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Ordinates;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Positioning;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Proofing;
using VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Time;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Time;
using VSS.VisionLink.Raptor.Types;
using Xunit;

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

    public class TAGValueMatcherTests
    {
        private static void InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state)
        {
            sink = new TAGProcessorStateBase_Test(); // TAGProcessorStateBase();
            state = new TAGValueMatcherState();
        }

        [Fact()]
        public void Test_TAGValueMatcher_ApplicationVersion()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGApplicationVersionValueMatcher(sink, state);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("Test")),
                "Matcher process function returned false");

            Assert.Equal(sink.ApplicationVersion, "Test");
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCATargetValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCATargetValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, sink.ICCCATargetValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCAValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCAValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, (byte) sink.ICCCAValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
                "Matcher process function returned false");

            Assert.Equal((byte) sink.ICCCAValues.GetLatest(), CellPass.NullCCA);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCARightFrontValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCARightFrontValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, (byte) sink.ICCCARightFrontValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
                "Matcher process function returned false");

            Assert.Equal((byte) sink.ICCCARightFrontValues.GetLatest(), CellPass.NullCCA);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCARightRearValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCARightRearValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, (byte) sink.ICCCARightRearValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
                "Matcher process function returned false");

            Assert.Equal((byte) sink.ICCCARightRearValues.GetLatest(), CellPass.NullCCA);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCALeftFrontValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCALeftFrontValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, (byte) sink.ICCCALeftFrontValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
                "Matcher process function returned false");

            Assert.Equal((byte) sink.ICCCALeftFrontValues.GetLatest(), CellPass.NullCCA);
        }


        [Fact()]
        public void Test_TAGValueMatcher_CCALeftRearValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCALeftRearValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, (byte) sink.ICCCALeftRearValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0)),
                "Matcher process function returned false");

            Assert.Equal((byte) sink.ICCCALeftRearValues.GetLatest(), CellPass.NullCCA);
        }


        [Fact()]
        public void Test_TAGValueMatcher_UsingCCA()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGUsingCCAValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    1),
                "Matcher process function returned false");

            Assert.True(sink.UsingCCA, "TAG value not processed as expected");

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    0),
                "Matcher process function returned false");

            Assert.False(sink.UsingCCA, "TAG value not processed as expected");
        }

        [Fact()]
        public void Test_TAGValueMatcher_CCVValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCCVValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (short) sink.ICCCVValues.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (short) sink.ICCCVValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteCCV, "Incorrect value after assignment");

            Assert.Equal((short) sink.ICCCVValues.GetLatest(), CellPass.NullCCV);
        }

        [Fact()]
        public void Test_TAGValueMatcher_TargetCCVValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTargetCCVValueMatcher(sink, state);

            Assert.Equal(sink.ICCCVTargetValue, CellPass.NullCCV);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, sink.ICCCVTargetValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MDPValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMDPValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (short) sink.ICMDPValues.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (short) sink.ICMDPValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteMDP, "Incorrect value after assignment");

            Assert.Equal((short) sink.ICMDPValues.GetLatest(), CellPass.NullMDP);
        }

        [Fact()]
        public void Test_TAGValueMatcher_TargetMDPValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTargetMDPValueMatcher(sink, state);

            Assert.Equal(sink.ICMDPTargetValue, CellPass.NullMDP);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, sink.ICMDPTargetValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_PassCount()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTargetPassCountValueMatcher(sink, state);

            Assert.Equal(0, sink.ICPassTargetValue);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(100, sink.ICPassTargetValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Temperature()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTemperatureValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (ushort) sink.ICTemperatureValues.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (ushort) sink.ICTemperatureValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteTemperature, "Incorrect value after assignment");

            Assert.Equal((ushort) sink.ICTemperatureValues.GetLatest(), CellPass.NullMaterialTemp);
        }

        [Fact()]
        public void Test_TAGValueMatcher_TemperatureWarningLevelMin()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTemperatureWarningLevelMinValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ICTempWarningLevelMinValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_TemperatureWarningLevelMax()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTemperatureWarningLevelMaxValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ICTempWarningLevelMaxValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Amplitude()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGAmplitudeValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (ushort) sink.ICAmplitudes.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (ushort) sink.ICAmplitudes.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteAmplitude, "Incorrect value after assignment");

            Assert.Equal((ushort) sink.ICAmplitudes.GetLatest(), CellPass.NullAmplitude);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Frequency()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGFrequencyValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (ushort) sink.ICFrequencys.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (ushort) sink.ICFrequencys.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteFrequency, "Incorrect value after assignment");

            Assert.Equal((ushort) sink.ICFrequencys.GetLatest(), CellPass.NullFrequency);
        }

        [Fact()]
        public void Test_TAGValueMatcher_RMVJumpthreshold()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGRMVJumpThresholdValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(100, sink.ICRMVJumpthreshold);
        }

        [Fact()]
        public void Test_TAGValueMatcher_RMV()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGRMVValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (short) sink.ICRMVValues.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (short) sink.ICRMVValues.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteRMV, "Incorrect value after assignment");

            Assert.Equal((short) sink.ICRMVValues.GetLatest(), CellPass.NullRMV);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ICModeFlags()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGICModeFlagsValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    8),
                "Matcher process function returned false");
            Assert.Equal(8, sink.ICMode);
        }

        [Fact()]
        public void Test_TAGValueMatcher_LayerID()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLayerIDValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    8),
                "Matcher process function returned false");
            Assert.Equal(8, sink.ICLayerIDValue);
        }

        [Fact()]
        public void Test_TAGValueMatcher_TargetLiftThickness()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTargetLiftThicknessValueMatcher(sink, state);

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t16bitUInt, 0),
                    1000), // supplied as millimeters
                "Matcher process function returned false");
            Assert.Equal(sink.ICTargetLiftThickness, 1.0); // Presented as meters
        }

        [Fact()]
        public void Test_TAGValueMatcher_VolkelMeasurementRangeUtil()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGVolkelMeasurementRangeUtilValueMatcher(sink, state);

            // Test offset value before absolute value sets correctly
            Assert.False(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Offset accepted before absolute");

            // Test value sets correctly
            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, (int) sink.VolkelMeasureUtilRanges.GetLatest());
            Assert.True(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

            Assert.True(matcher.ProcessIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0),
                    100),
                "Matcher process function returned false");

            Assert.Equal(1100, (int) sink.VolkelMeasureUtilRanges.GetLatest());

            // Test value ussets correctly on an empty value
            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.t8bitInt, 0)),
                "Matcher process function returned false");

            Assert.False(state.HaveSeenAnAbsoluteVolkelMeasUtilRange, "Incorrect value after assignment");

            Assert.Equal((int) sink.VolkelMeasureUtilRanges.GetLatest(), CellPass.NullVolkelMeasUtilRange);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ControlStateLeftLift()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGControlStateLeftLiftValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ControlStateLeftLift);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ControlStateRightLift()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGControlStateRightLiftValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ControlStateRightLift);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ControlStateLift()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            TAGControlStateLiftValueMatcher matcher = new TAGControlStateLiftValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ControlStateLift);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ControlStateTilt()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGControlStateTiltValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ControlStateTilt);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ControlStateSideShift()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGControlStateSideShiftValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t12bitUInt, 0),
                    1000),
                "Matcher process function returned false");
            Assert.Equal(1000, sink.ControlStateSideShift);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CoordinateSystemType()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGCoordinateSystemTypeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    0), // Unknown CS
                "Matcher process function returned false");
            Assert.Equal(sink.CSType, CoordinateSystemType.NoCoordSystem);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    1), // Project calibration (CSIB)
                "Matcher process function returned false");
            Assert.Equal(sink.CSType, CoordinateSystemType.CSIB);
            Assert.True(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    2), // Automatic Coordinate System (ACS)
                "Matcher process function returned false");
            Assert.Equal(sink.CSType, CoordinateSystemType.ACS);
            Assert.False(sink.IsCSIBCoordSystemTypeOnly, "Incorrect value after assignment");

            Assert.False(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    3), // Invalid
                "Matcher process function returned false");
        }


        [Fact()]
        public void Test_TAGValueMatcher_UTMZone()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGUTMZoneValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    50),
                "Matcher process function returned false");
            Assert.Equal(50, sink.UTMZone);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MachineShutdown()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMachineShutdownValueMatcher(sink, state);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(((TAGProcessorStateBase_Test) sink).TriggeredEpochStateEvent, EpochStateEvent.MachineShutdown);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MachineStartup()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMachineStartupValueMatcher(sink, state);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(((TAGProcessorStateBase_Test) sink).TriggeredEpochStateEvent, EpochStateEvent.MachineStartup);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MapReset()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMapResetValueMatcher(sink, state);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(((TAGProcessorStateBase_Test) sink).TriggeredEpochStateEvent, EpochStateEvent.MachineMapReset);
        }

        [Fact()]
        public void Test_TAGValueMatcher_UTSMode()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGUTSModeValueMatcher(sink, state);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(((TAGProcessorStateBase_Test) sink).TriggeredEpochStateEvent,
                EpochStateEvent.MachineInUTSMode);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Height()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGHeightValueMatcher(sink, state);

            Assert.True(matcher.ProcessDoubleValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
                "Matcher process function returned false");
            Assert.Equal(sink.LLHHeight, 100.0);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Longitude()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLongitudeValueMatcher(sink, state);

            Assert.True(matcher.ProcessDoubleValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
                "Matcher process function returned false");
            Assert.Equal(sink.LLHLon, 100.0);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Latitude()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLatitudeValueMatcher(sink, state);

            Assert.True(matcher.ProcessDoubleValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
                "Matcher process function returned false");
            Assert.Equal(sink.LLHLat, 100.0);
        }

        [Fact()]
        public void Test_TAGValueMatcher_CompactorSensorType()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            TAGCompactorSensorTypeValueMatcher matcher = new TAGCompactorSensorTypeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    0), // No sensor
                "Matcher process function returned false");
            Assert.Equal(sink.ICSensorType, CompactionSensorType.NoSensor);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    1), // MC 024
                "Matcher process function returned false");
            Assert.Equal(sink.ICSensorType, CompactionSensorType.MC024);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    2), // Volkel
                "Matcher process function returned false");
            Assert.Equal(sink.ICSensorType, CompactionSensorType.Volkel);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    3), // CAT factory fit
                "Matcher process function returned false");
            Assert.Equal(sink.ICSensorType, CompactionSensorType.CATFactoryFitSensor);

            Assert.False(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    4), // Invalid
                "Matcher process function returned false");
        }

        [Fact()]
        public void Test_TAGValueMatcher_VolkelMeasurementRange()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGVolkelMeasurementRangeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(100, (int) sink.VolkelMeasureRanges.GetLatest());
        }

        [Fact()]
        public void Test_TAGValueMatcher_RadioSerial()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGRadioSerialValueMatcher(sink, state);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("Test")),
                "Matcher process function returned false");

            Assert.Equal(sink.RadioSerial, "Test");
        }

        [Fact()]
        public void Test_TAGValueMatcher_RadioType()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGRadioTypeValueMatcher(sink, state);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("Test")),
                "Matcher process function returned false");

            Assert.Equal(sink.RadioType, "Test");
        }

        [Fact()]
        public void Test_TAGValueMatcher_BladeOnGround()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGBladeOnGroundValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    0), // No
                "Matcher process function returned false");

            Assert.Equal((OnGroundState) sink.OnGrounds.GetLatest(), OnGroundState.No);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    5), //Yes, Remote Switch
                "Matcher process function returned false");

            Assert.Equal((OnGroundState) sink.OnGrounds.GetLatest(), OnGroundState.YesRemoteSwitch);

            Assert.False(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    7), // Invalid
                "Matcher process function returned false");
        }

        [Fact()]
        public void Test_TAGValueMatcher_OnGround()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGOnGroundValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    0), // No
                "Matcher process function returned false");

            Assert.Equal((OnGroundState) sink.OnGrounds.GetLatest(), OnGroundState.No);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    1), //Yes, Legacy
                "Matcher process function returned false");

            Assert.Equal((OnGroundState) sink.OnGrounds.GetLatest(), OnGroundState.YesLegacy);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    2), // Invalid -> unknown
                "Matcher process function returned false");
            Assert.Equal((OnGroundState) sink.OnGrounds.GetLatest(), OnGroundState.Unknown);
        }

        [Fact()]
        public void Test_TAGValueMatcher_Design()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGDesignValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnicodeStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    "Test"),
                "Matcher process function returned false");

            Assert.Equal(sink.Design, "Test");
        }

        [Fact()]
        public void Test_TAGValueMatcher_Gear()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGGearValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    3), // Sensor failed
                "Matcher process function returned false");
            Assert.Equal(sink.ICGear, MachineGear.SensorFailedDeprecated);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MachineID()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMachineIDValueMatcher(sink, state);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("Test")),
                "Matcher process function returned false");

            Assert.Equal(sink.MachineID, "Test");
        }

        [Fact()]
        public void Test_TAGValueMatcher_MachineSpeed()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMachineSpeedValueMatcher(sink, state);

            Assert.True(matcher.ProcessDoubleValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0), 100.0),
                "Matcher process function returned false");
            Assert.Equal((double) sink.ICMachineSpeedValues.GetLatest(), 100.0);
            Assert.True(state.HaveSeenMachineSpeed, "HaveSeenMachineSpeed not set");
        }

        [Fact()]
        public void Test_TAGValueMatcher_MachineType()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMachineTypeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t8bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(100, sink.MachineType);
        }

        [Fact()]
        public void Test_TAGValueMatcher_MinElevMapping()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGMinElevMappingValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    0),
                "Matcher process function returned false");
            Assert.False(sink.MinElevMapping);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    1),
                "Matcher process function returned false");
            Assert.True(sink.MinElevMapping);

            Assert.False(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    2),
                "Matcher process function returned false");
        }

        [Fact()]
        public void Test_TAGValueMatcher_Sequence()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGSequenceValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t32bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal((uint) 100, sink.Sequence);
        }

        [Fact()]
        public void Test_TAGValueMatcher_WheelWidth()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGWheelWidthValueMatcher(sink, state);

            Assert.True(matcher.ProcessDoubleValue(new TAGDictionaryItem("", TAGDataType.tIEEEDouble, 0),
                    1.0),
                "Matcher process function returned false");
            Assert.Equal(sink.MachineWheelWidth, 1.0);
        }

        [Fact()]
        public void Test_TAGValueMatcher_LeftRightBlade()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLeftRightBladeValueMatcher(sink, state);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftTag, TAGDataType.tEmptyType,
                    0)),
                "Matcher process function returned false");
            Assert.Equal(state.Side, TAGValueSide.Left);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightTag, TAGDataType.tEmptyType,
                    0)),
                "Matcher process function returned false");
            Assert.Equal(state.Side, TAGValueSide.Right);
        }

        [Fact()]
        public void Test_TAGValueMatcher_LeftRightTrack()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLeftRightTrackValueMatcher(sink, state);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftTrackTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.TrackSide, TAGValueSide.Left);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightTrackTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.TrackSide, TAGValueSide.Right);
        }

        [Fact()]
        public void Test_TAGValueMatcher_LeftRightWheel()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLeftRightWheelValueMatcher(sink, state);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftWheelTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.WheelSide, TAGValueSide.Left);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightWheelTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.WheelSide, TAGValueSide.Right);
        }

        [Fact()]
        public void Test_TAGValueMatcher_LeftRightRear()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGLeftRightRearValueMatcher(sink, state);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileLeftRearTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.RearSide, TAGValueSide.Left);

            Assert.True(
                matcher.ProcessEmptyValue(new TAGDictionaryItem(TAGValueNames.kTagFileRightRearTag,
                    TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.Equal(state.RearSide, TAGValueSide.Right);
        }

        [Fact()]
        public void Test_TAGValueMatcher_BladeOrdinateValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGBladeOrdinateValueMatcher(sink, state);

            state.Side = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingTag, TAGDataType.t32bitInt, 0);

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataLeft.X, 1.0);
            Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileNorthingTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataLeft.Y, 1.0);
            Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileElevationTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataLeft.Z, 1.0);
            Assert.True(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataLeft.Z, 1.5);

            // Test an empty value clears the absolute value seen flag

            Assert.True(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.False(state.HaveSeenAnAbsolutePosition, "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_TrackOrdinateValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTrackOrdinateValueMatcher(sink, state);

            state.TrackSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingTrackTag, TAGDataType.t32bitInt, 0);

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataTrackLeft.X, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataTrackLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileNorthingTrackTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingTrackTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataTrackLeft.Y, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataTrackLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileElevationTrackTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationTrackTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataTrackLeft.Z, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataTrackLeft.Z, 1.5);

            // Test an empty value clears the absolute value seen flag

            Assert.True(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.False(state.HaveSeenAnAbsoluteTrackPosition, "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_WheelOrdinateValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGWheelOrdinateValueMatcher(sink, state);

            state.WheelSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingWheelTag, TAGDataType.t32bitInt, 0);

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataWheelLeft.X, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataWheelLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileNorthingWheelTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingWheelTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataWheelLeft.Y, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataWheelLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileElevationWheelTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationWheelTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataWheelLeft.Z, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataWheelLeft.Z, 1.5);

            // Test an empty value clears the absolute value seen flag

            Assert.True(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.False(state.HaveSeenAnAbsoluteWheelPosition, "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_RearOrdinateValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGRearOrdinateValueMatcher(sink, state);

            state.RearSide = TAGValueSide.Left;

            TAGDictionaryItem absoluteItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, TAGDataType.tIEEEDouble, 0);
            TAGDictionaryItem offsetItem =
                new TAGDictionaryItem(TAGValueNames.kTagFileEastingRearTag, TAGDataType.t32bitInt, 0);

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataRearLeft.X, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataRearLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileNorthingRearTag;
            offsetItem.Name = TAGValueNames.kTagFileNorthingRearTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataRearLeft.Y, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataRearLeft.X, 1.5);

            absoluteItem.Name = TAGValueNames.kTagFileElevationRearTag;
            offsetItem.Name = TAGValueNames.kTagFileElevationRearTag;

            Assert.True(matcher.ProcessDoubleValue(absoluteItem, 1.0), "Matcher process function returned false");

            Assert.Equal(sink.DataRearLeft.Z, 1.0);
            Assert.True(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");

            // Offset the absolute item with a distance in millimeters
            Assert.True(matcher.ProcessIntegerValue(offsetItem, 500), "Matcher process function returned false");
            Assert.Equal(sink.DataRearLeft.Z, 1.5);

            // Test an empty value clears the absolute value seen flag

            Assert.True(matcher.ProcessEmptyValue(absoluteItem), "Matcher process function returned false");
            Assert.False(state.HaveSeenAnAbsoluteRearPosition, "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_GPSAccuracy()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGGPSAccuracyValueMatcher(sink, state);

            // Set GPS accuracy word to be Medium accuracy with a 100mm error limit
            ushort GPSAccuracyWord = (0x1 << 14) | 100;

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t16bitUInt, 0),
                    GPSAccuracyWord),
                "Matcher process function returned false");
            Assert.True(sink.GPSAccuracy == GPSAccuracy.Medium && sink.GPSAccuracyErrorLimit == 100,
                "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_GPSBasePosition()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGGPSBasePositionValueMatcher(sink, state);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tEmptyType, 0)),
                "Matcher process function returned false");
            Assert.True(state.GPSBasePositionReportingHaveStarted, "Incorrect value after assignment");
        }

        [Fact()]
        public void Test_TAGValueMatcher_GPSMode()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGGPSModeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    2), // Float RTK
                "Matcher process function returned false");
            Assert.Equal((GPSMode) sink.GPSModes.GetLatest(), GPSMode.Float);
        }

        [Fact()]
        public void Test_TAGValueMatcher_ValidPosition()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGValidPositionValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    100),
                "Matcher process function returned false");
            Assert.Equal(100, sink.ValidPosition);
        }

        [Fact()]
        public void Test_TAGValueMatcher_EndProofingTimeValue()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGEndProofingTimeValueMatcher(sink, state);

            // Test the Time aspect
            Assert.True(matcher.ProcessUnsignedIntegerValue(
                    new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingTimeTag, TAGDataType.t32bitUInt, 0),
                    10000000),
                "Matcher process function returned false");
            Assert.Equal((uint) 10000000, sink.StartProofingTime);
            Assert.True(state.HaveSeenAProofingRunTimeValue, "Incorrect value after assignment");

            // Test the Week aspect
            Assert.True(matcher.ProcessUnsignedIntegerValue(
                    new TAGDictionaryItem(TAGValueNames.kTagFileStartProofingWeekTag, TAGDataType.t16bitUInt, 0),
                    10000),
                "Matcher process function returned false");
            Assert.Equal(10000, sink.StartProofingWeek);
            Assert.True(state.HaveSeenAProofingRunWeekValue, "Incorrect value after assignment");

            // Test the actual start proofing run time value is calculated correctly
            Assert.True(sink.StartProofingTime == 10000000 &&
                        sink.StartProofingWeek == 10000 &&
                        GPS.GPSOriginTimeToDateTime(sink.StartProofingWeek, sink.StartProofingTime) ==
                        sink.StartProofingDataTime,
                "Start proofing data time not as expected");
        }

        [Fact()]
        public void Test_TAGValueMatcher_EndProofing()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGEndProofingValueMatcher(sink, state);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("TestEnd")),
                "Matcher process function returned false");

            Assert.Equal(sink.EndProofingName, "TestEnd");
        }

        [Fact()]
        public void Test_TAGValueMatcher_StartProofing()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGStartProofingValueMatcher(sink, state);

            // Set time and week values
            sink.GPSWeekTime = 10000000;
            state.HaveSeenATimeValue = true;
            sink.GPSWeekNumber = 10000;
            state.HaveSeenAWeekValue = true;

            sink.DataTime = GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime = 10000000);

            Assert.True(matcher.ProcessANSIStringValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0),
                    Encoding.ASCII.GetBytes("TestStart")),
                "Matcher process function returned false");

            Assert.Equal(sink.StartProofing, "TestStart");

            // Test the start proofing data time is calculated correctly
            Assert.Equal(sink.StartProofingDataTime, sink.DataTime);

            Assert.True(matcher.ProcessEmptyValue(new TAGDictionaryItem("", TAGDataType.tANSIString, 0)),
                "Matcher process function returned false");

            Assert.Equal(sink.StartProofing, "");
        }

        [Fact()]
        public void Test_TAGValueMatcher_Time()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGTimeValueMatcher(sink, state);

            // Test the Time aspect
            Assert.True(matcher.ProcessUnsignedIntegerValue(
                    new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, TAGDataType.t32bitUInt, 0),
                    10000000),
                "Matcher process function returned false");
            Assert.Equal((uint) 10000000, sink.GPSWeekTime);
            Assert.True(state.HaveSeenATimeValue, "Incorrect HaveSeenATimeValue value after time assignment");
            Assert.False(state.HaveSeenAWeekValue, "Incorrect HaveSeenAWeekValue value after time assignment");

            // Test the absolute Week aspect
            Assert.True(matcher.ProcessUnsignedIntegerValue(
                    new TAGDictionaryItem(TAGValueNames.kTagFileWeekTag, TAGDataType.t16bitUInt, 0),
                    10000),
                "Matcher process function returned false");
            Assert.Equal(10000, sink.GPSWeekNumber);
            Assert.True(state.HaveSeenAWeekValue, "Incorrect value after assignment");

            // Test the epoch data time is calculated correctly
            Assert.True(sink.GPSWeekTime == 10000000 &&
                        sink.GPSWeekNumber == 10000 &&
                        GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
                "Absolute epoch GPS data time not as expected");

            // Test the incremental time aspect
            Assert.True(matcher.ProcessUnsignedIntegerValue(
                    new TAGDictionaryItem(TAGValueNames.kTagFileTimeTag, TAGDataType.t4bitUInt, 0),
                    10), // Adds 10, 1/10 of a second intervals, for an increment of 1000ms
                "Matcher process function returned false");
            Assert.Equal((uint) 10001000, sink.GPSWeekTime);
            Assert.True(state.HaveSeenAWeekValue, "Incorrect value after assignment");

            // Test the epoch data time is calculated correctly
            Assert.True(sink.GPSWeekTime == 10001000 &&
                        sink.GPSWeekNumber == 10000 &&
                        GPS.GPSOriginTimeToDateTime(sink.GPSWeekNumber, sink.GPSWeekTime) == sink.DataTime,
                "Incremented epoch GPS data time not as expected");
        }

        [Fact()]
        public void Test_TAGValueMatcher_AgeOfCOrrections()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGAgeValueMatcher(sink, state);

            Assert.True(matcher.ProcessUnsignedIntegerValue(new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0),
                    10),
                "Matcher process function returned false");
            Assert.Equal(10, (byte) sink.AgeOfCorrections.GetLatest());
        }

        [Fact()]
        public void Test_TAGValueMatcher_3DSonic()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAG3DSonicValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

            for (uint i = 0; i < 3; i++)
            {
                Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, i),
                    "Matcher process function returned false");
                Assert.Equal(sink.ICSonic3D, i);
            }

            Assert.False(matcher.ProcessUnsignedIntegerValue(dictItem, 3),
                "Matcher process function returned false");
        }

        [Fact()]
        public void Test_TAGValueMatcher_Direction()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGDirectionValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

            // Note: Machien direction values from the machine are 1-based
            Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, 1), // Forwards
                "Matcher process function returned false");
            Assert.Equal(sink.MachineDirection, MachineDirection.Forward);

            Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, 2), // Reverse
                "Matcher process function returned false");
            Assert.Equal(sink.MachineDirection, MachineDirection.Reverse);
        }

        [Fact()]
        public void Test_TAGValueMatcher_AvoidanceZone()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGInAvoidanceZoneValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

            for (uint i = 0; i < 4; i++)
            {
                Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, i),
                    "Matcher process function returned false");
                Assert.Equal(sink.InAvoidZone, i);
            }

            Assert.False(matcher.ProcessUnsignedIntegerValue(dictItem, 5),
                "Matcher process function returned false");
        }

        [Fact()]
        public void Test_TAGValueMatcher_ResearchData()
        {

            InitStateAndSink(out TAGProcessorStateBase sink, out TAGValueMatcherState state);
            var matcher = new TAGResearchDataValueMatcher(sink, state);
            var dictItem = new TAGDictionaryItem("", TAGDataType.t4bitUInt, 0);

            Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, 0), // False
                "Matcher process function returned false");
            Assert.False(sink.ResearchData);

            Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, 1), // False
                "Matcher process function returned false");
            Assert.True(sink.ResearchData);

            Assert.True(matcher.ProcessUnsignedIntegerValue(dictItem, 2), // Also true...
                "Matcher process function returned false");
            Assert.True(sink.ResearchData);
        }
    }
}