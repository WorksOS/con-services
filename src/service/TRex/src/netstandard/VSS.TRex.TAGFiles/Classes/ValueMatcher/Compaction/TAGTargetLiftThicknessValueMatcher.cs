using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction
{
    /// <summary>
    /// Handles Target Thinkness vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetLiftThicknessValueMatcher : TAGValueMatcher
    {
        public TAGTargetLiftThicknessValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileTargetLiftThickness };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t16bitUInt)
            {
                valueSink.ICTargetLiftThickness = (float) value / 1000;
                return true;
            }

            return false;
        }
    }
}
