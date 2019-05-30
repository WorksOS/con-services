using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGDesignValueMatcher : TAGValueMatcher
    {
        public TAGDesignValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileDesignTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnicodeStringValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, string value)
        {
            valueSink.Design = value;

            return true;
        }
    }
}
