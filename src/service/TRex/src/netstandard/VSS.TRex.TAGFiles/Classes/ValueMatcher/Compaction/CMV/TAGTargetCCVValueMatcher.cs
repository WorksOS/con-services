using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CMV
{
    /// <summary>
    /// Handles Target CCV vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetCCVValueMatcher : TAGValueMatcher
    {
        public TAGTargetCCVValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCVTargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICCCVTargetValue = (short)value;
                result = true;
            }

            return result;            
        }
    }
}
