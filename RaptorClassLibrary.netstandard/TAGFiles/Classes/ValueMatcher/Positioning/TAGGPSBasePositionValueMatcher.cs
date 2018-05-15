namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    public class TAGGPSBasePositionValueMatcher : TAGValueMatcher
    {
        public TAGGPSBasePositionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagGPSBasePosition };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.GPSBasePositionReportingHaveStarted = true;

            return true;
        }
    }
}
