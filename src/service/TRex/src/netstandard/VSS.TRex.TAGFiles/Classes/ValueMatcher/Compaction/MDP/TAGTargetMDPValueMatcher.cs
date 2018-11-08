using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.MDP
{
    /// <summary>
    /// Handles Target MDP vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetMDPValueMatcher : TAGValueMatcher
    {
        public TAGTargetMDPValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICMDPTargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is the absolute MDP value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICMDPTargetValue = (short) value;
                return true;
            }

            return false;
        }
    }
}
