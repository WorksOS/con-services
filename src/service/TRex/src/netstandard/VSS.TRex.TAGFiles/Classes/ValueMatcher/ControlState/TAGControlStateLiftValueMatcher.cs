using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateLiftValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateLift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateLift = (int)value;

            return true;
        }
    }
}
