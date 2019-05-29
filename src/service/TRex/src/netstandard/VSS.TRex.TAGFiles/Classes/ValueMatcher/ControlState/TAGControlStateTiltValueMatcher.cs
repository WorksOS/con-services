using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateTiltValueMatcher : TAGValueMatcher
    {
        public TAGControlStateTiltValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateTilt };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateTilt = (int)value;

            return true;
        }
    }
}
