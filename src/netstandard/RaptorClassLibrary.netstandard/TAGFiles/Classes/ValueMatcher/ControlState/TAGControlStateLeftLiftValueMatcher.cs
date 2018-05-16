namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateLeftLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateLeftLiftValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateLeftLift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateLeftLift = (int)value;

            return true;
        }
    }
}
