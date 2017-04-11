using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Location
{
    /// <summary>
    /// Handles a latitude position (such as TAG file seed location) reported by a machine
    /// </summary>
    public class TAGLatitudeValueMatcher : TAGValueMatcher
    {
        public TAGLatitudeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagPositionLatitude };
        }

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            if (state.GPSBasePositionReportingHaveStarted)
            {
                valueSink.GPSBaseLat = value;
                state.GPSBasePositionReportingHaveStarted = !valueSink.GPSBaseLLHReceived;
            }
            else
            {
                valueSink.LLHLat = value;
                state.HaveSeenAnLLHPosition = valueSink.LLHReceived;
            }

            return true;
        }
    }
}
