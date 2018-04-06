namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGDesignValueMatcher : TAGValueMatcher
    {
        public TAGDesignValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileDesignTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnicodeStringValue(TAGDictionaryItem valueType, string value)
        {
            valueSink.Design = value;

            return true;
        }
    }
}
