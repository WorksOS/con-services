using System.Text;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Proofing
{
    /// <summary>
    /// Handles proofing run ending TAGs
    /// </summary>
    public class TAGEndProofingValueMatcher : TAGValueMatcher
    {
        public TAGEndProofingValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileEndProofingNameTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGDictionaryItem valueType, byte[] value)
        {
            valueSink.EndProofingName = Encoding.ASCII.GetString(value);

            return true;
        }
    }
}
