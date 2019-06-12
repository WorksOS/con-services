using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.ControlState
{
    public class TAGControlStateLeftLiftValueMatcher : TAGValueMatcher
    {
        public TAGControlStateLeftLiftValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileControlStateLeftLift };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            valueSink.ControlStateLeftLift = (int)value;

            return true;
        }
    }
}
