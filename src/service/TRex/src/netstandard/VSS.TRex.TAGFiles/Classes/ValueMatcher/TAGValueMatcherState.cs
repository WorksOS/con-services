using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// A collection of state flags used to augment the processing of TAG values through the TAG value sink
    /// </summary>
    public class TAGValueMatcherState
    {
        public TAGValueSide Side;
        public TAGValueSide TrackSide;
        public TAGValueSide WheelSide;
        public TAGValueSide RearSide;
        public bool HaveSeenATimeValue;
        public bool HaveSeenAWeekValue;
        public bool HaveSeenAProofingRunTimeValue;
        public bool HaveSeenAProofingRunWeekValue;
        public bool ProofingRunProcessed;
        public bool HaveSeenAMachineID;
        public bool HaveSeenAnAbsolutePosition;
        public bool HaveSeenAnAbsoluteTrackPosition;
        public bool HaveSeenAnAbsoluteWheelPosition;
        public bool HaveSeenAnAbsoluteRearPosition;
        public bool HaveSeenAnAbsoluteCCV;
        public bool HaveSeenAnAbsoluteFrequency;
        public bool HaveSeenAnAbsoluteAmplitude;
        public bool HaveSeenAnAbsoluteRMV;
        public bool HaveSeenAnAbsoluteMDP;
        public bool HaveSeenAnAbsoluteTemperature;
        public bool HaveSeenARadioSerial;
        public bool HaveSeenARadioType;
        public bool HaveSeenAnLLHPosition;
        public bool HaveSeenAUTMZone;
        public bool HaveSeenAnAbsoluteVolkelMeasUtilRange;
        public bool HaveSeenMachineSpeed;
        public bool GPSBasePositionReportingHaveStarted;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TAGValueMatcherState()
        {
            ProofingRunProcessed = true;
        }
    }
}
