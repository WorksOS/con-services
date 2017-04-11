using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool HaveSeenATimeValue { get; set; } = false;
        public bool HaveSeenAWeekValue { get; set; } = false;
        public bool HaveSeenAProofingRunTimeValue { get; set; } = false;
        public bool HaveSeenAProofingRunWeekValue { get; set; } = false;
        public bool ProofingRunProcessed { get; set; } = true;
        public bool HaveSeenAMachineID { get; set; } = false;
        public bool HaveSeenAnAbsolutePosition { get; set; } = false;
        public bool HaveSeenAnAbsoluteTrackPosition { get; set; } = false;
        public bool HaveSeenAnAbsoluteWheelPosition { get; set; } = false;
        public bool HaveSeenAnAbsoluteRearPosition { get; set; } = false;
        public bool HaveSeenAnAbsoluteCCV { get; set; } = false;
        public bool HaveSeenAnAbsoluteFrequency { get; set; } = false;
        public bool HaveSeenAnAbsoluteAmplitude { get; set; } = false;
        public bool HaveSeenAnAbsoluteRMV { get; set; } = false;
        public bool HaveSeenAnAbsoluteMDP { get; set; } = false;
        public bool HaveSeenAnAbsoluteTemperature { get; set; } = false;
        public bool HaveSeenARadioSerial { get; set; } = false;
        public bool HaveSeenARadioType { get; set; } = false;
        public bool HaveSeenAnLLHPosition { get; set; } = false;
        public bool HaveSeenAUTMZone { get; set; } = false;
        public bool HaveSeenAnAbsoluteVolkelMeasUtilRange { get; set; } = false;
        public bool HaveSeenMachineSpeed { get; set; } = false;
        public bool GPSBasePositionReportingHaveStarted { get; set; } = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TAGValueMatcherState()
        {

        }
    }
}
