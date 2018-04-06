namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Location
{
    /// <summary>
    /// Handles a height (such as TAG file seed location) reported by a machine
    /// </summary>
    public class TAGHeightValueMatcher : TAGValueMatcher
    {
        public TAGHeightValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagPositionHeight };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessDoubleValue(TAGDictionaryItem valueType, double value)
        {
            if (state.GPSBasePositionReportingHaveStarted)
            {
                valueSink.GPSBaseHeight = value;
                state.GPSBasePositionReportingHaveStarted = !valueSink.GPSBaseLLHReceived;
            }
            else
            {
                valueSink.LLHHeight = value;
                state.HaveSeenAnLLHPosition = valueSink.LLHReceived;
            }

            return true;
        }
    }
}
