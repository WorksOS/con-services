namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateTiltValueMatcher : TAGValueMatcher
    {
        public TAGControlStateTiltValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateTilt };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateTilt = (int)value;

            return true;
        }
    }
}
