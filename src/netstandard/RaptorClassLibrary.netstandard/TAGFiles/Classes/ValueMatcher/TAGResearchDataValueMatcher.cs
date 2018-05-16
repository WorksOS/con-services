namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// Handles the machine control system research data flag
    /// </summary>
    public class TAGResearchDataValueMatcher : TAGValueMatcher
    {
        public TAGResearchDataValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(
            valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagResearchData };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            valueSink.ResearchData = value != 0;

            return true;
        }
    }
}
