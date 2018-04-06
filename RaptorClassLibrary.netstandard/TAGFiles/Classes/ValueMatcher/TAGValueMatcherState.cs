using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// A collection of state flags used to augment the processing of TAG values through the TAG value sink
    /// </summary>
    public class TAGValueMatcherState
    {
        public TAGValueSide Side { get; set; } = TAGValueSide.Left;
        public TAGValueSide TrackSide { get; set; } = TAGValueSide.Left;
        public TAGValueSide WheelSide { get; set; } = TAGValueSide.Left;
        public TAGValueSide RearSide { get; set; } = TAGValueSide.Left;
        public bool HaveSeenATimeValue { get; set; }
        public bool HaveSeenAWeekValue { get; set; }
        public bool HaveSeenAProofingRunTimeValue { get; set; }
        public bool HaveSeenAProofingRunWeekValue { get; set; }
        public bool ProofingRunProcessed { get; set; } = true;
        public bool HaveSeenAMachineID { get; set; }
        public bool HaveSeenAnAbsolutePosition { get; set; }
        public bool HaveSeenAnAbsoluteTrackPosition { get; set; }
        public bool HaveSeenAnAbsoluteWheelPosition { get; set; }
        public bool HaveSeenAnAbsoluteRearPosition { get; set; }
        public bool HaveSeenAnAbsoluteCCV { get; set; }
        public bool HaveSeenAnAbsoluteFrequency { get; set; }
        public bool HaveSeenAnAbsoluteAmplitude { get; set; }
        public bool HaveSeenAnAbsoluteRMV { get; set; }
        public bool HaveSeenAnAbsoluteMDP { get; set; }
        public bool HaveSeenAnAbsoluteTemperature { get; set; }
        public bool HaveSeenARadioSerial { get; set; }
        public bool HaveSeenARadioType { get; set; } = false;
        public bool HaveSeenAnLLHPosition { get; set; }
        public bool HaveSeenAUTMZone { get; set; }
        public bool HaveSeenAnAbsoluteVolkelMeasUtilRange { get; set; }
        public bool HaveSeenMachineSpeed { get; set; }
        public bool GPSBasePositionReportingHaveStarted { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TAGValueMatcherState()
        {
        }
    }
}
