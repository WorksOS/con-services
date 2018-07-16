using VSS.TRex.TAGFiles.Classes.States;

namespace VSS.TRex.TAGFiles.Classes.Sinks
{
    /// <summary>
    /// Derived TAG value sink that just looks for the first instance of GPS Week and Time values before completing
    /// </summary>
    public class TAGStartTimeValueSink : TAGValueSink
    {
        /// <summary>
        /// Constructor -> proxied to base
        /// </summary>
        /// <param name="processor"></param>
        public TAGStartTimeValueSink(TAGProcessorStateBase processor) : base(processor)
        {
        }

        /// <summary>
        /// Abort the scan when a time and week record have been observed.
        /// </summary>
        /// <returns></returns>
        public override bool Aborting()
        {
            return ValueMatcherState.HaveSeenATimeValue && ValueMatcherState.HaveSeenAWeekValue;
        }

        /// <summary>
        /// Default finishing behaviour
        /// </summary>
        /// <returns></returns>
        public override bool Finishing()
        {
            return true;
        }
    }
}
