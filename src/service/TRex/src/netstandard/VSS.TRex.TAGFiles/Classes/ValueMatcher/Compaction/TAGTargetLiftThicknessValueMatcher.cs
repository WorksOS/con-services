using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction
{
    /// <summary>
    /// Handles Target Thickness values set on the machine by an operator
    /// </summary>
    public class TAGTargetLiftThicknessValueMatcher : TAGValueMatcher
    {
        public TAGTargetLiftThicknessValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileTargetLiftThickness };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType) => true;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t16bitUInt)
            {
                valueSink.ICTargetLiftThickness = (float) value / 1000;
                result = true;
            }

            return result;
        }
    }
}
