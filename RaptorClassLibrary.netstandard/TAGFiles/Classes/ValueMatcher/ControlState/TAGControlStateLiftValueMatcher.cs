namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateLiftValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateLift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateLift = (int)value;

            return true;
        }
    }
}
