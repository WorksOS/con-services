using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.MDP
{
    /// <summary>
    /// Handles Target MDP vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetMDPValueMatcher : TAGValueMatcher
    {
        public TAGTargetMDPValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICMDPTargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            // Value is the absolute MDP value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICMDPTargetValue = (short) value;
                result = true;
            }

            return result;
        }
    }
}
