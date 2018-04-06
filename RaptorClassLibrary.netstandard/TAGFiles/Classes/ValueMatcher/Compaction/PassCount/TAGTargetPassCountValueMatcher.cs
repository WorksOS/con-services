using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.PassCount
{
    /// <summary>
    /// Handles Target CCV vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetPassCountValueMatcher : TAGValueMatcher
    {
        public TAGTargetPassCountValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICPassTargetTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICPassTargetValue = (ushort)value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
