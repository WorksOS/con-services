namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    /// <summary>
    /// Handle the flag indicating the compactor machine is using the Caterpillar Compaction Algorithm
    /// </summary>
    public class TAGUsingCCAValueMatcher : TAGValueMatcher
    {
        public TAGUsingCCAValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagUsingCCA };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.UsingCCA = value != 0;

            return true;
        }
    }
}
