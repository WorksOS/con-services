using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Location
{
    /// <summary>
    /// Handles a latitude position (such as TAG file seed location) reported by a machine
    /// </summary>
    public class TAGLongitudeValueMatcher : TAGValueMatcher
    {
        public TAGLongitudeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagPositionLongitude };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessDoubleValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, double value)
        {
            if (state.GPSBasePositionReportingHaveStarted)
            {
                valueSink.GPSBaseLon = value;
                state.GPSBasePositionReportingHaveStarted = !valueSink.GPSBaseLLHReceived;
            }
            else
            {
                valueSink.LLHLon = value;
                state.HaveSeenAnLLHPosition = valueSink.LLHReceived;
            }

            return true;
        }
    }
}
