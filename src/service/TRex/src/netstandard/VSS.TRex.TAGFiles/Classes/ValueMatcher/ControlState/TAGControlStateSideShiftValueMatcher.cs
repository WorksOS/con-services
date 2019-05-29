using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateSideShiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateSideShiftValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateSideShift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateSideShift = (int)value;

            return true;
        }
    }
}
