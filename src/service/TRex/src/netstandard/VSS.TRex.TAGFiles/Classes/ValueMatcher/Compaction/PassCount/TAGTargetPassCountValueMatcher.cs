using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.PassCount
{
    /// <summary>
    /// Handles Target CCV values set on the machine by an operator
    /// </summary>
    public class TAGTargetPassCountValueMatcher : TAGValueMatcher
    {
        public TAGTargetPassCountValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICPassTargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICPassTargetValue = (ushort) value;
                result = true;
            }

            return result;
        }
    }
}
