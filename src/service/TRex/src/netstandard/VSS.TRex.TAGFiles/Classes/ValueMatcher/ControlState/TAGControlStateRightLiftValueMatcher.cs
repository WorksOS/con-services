using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateRightLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateRightLiftValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateRightLift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateRightLift = (int)value;

            return true;
        }
    }
}
