using System.Text;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Proofing
{
    /// <summary>
    /// Handles proofing run ending TAGs
    /// </summary>
    public class TAGEndProofingValueMatcher : TAGValueMatcher
    {
        public TAGEndProofingValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileEndProofingNameTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessANSIStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, string value)
        {
            valueSink.EndProofingName = value;

            return true;
        }
    }
}
