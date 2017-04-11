using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Sinks
{ 
    /// <summary>
    /// Derived TAG value sink that just looks for the first instance of GPS Week and Time values before completing
    /// </summary>
    class TAGStartTimeValueSink : TAGValueSink
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
