using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    public class TAGValidPositionValueMatcher : TAGValueMatcher
    {
        public TAGValidPositionValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileValidPositionTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.ValidPosition = (byte)value;
                return true;
            }

            return false;
        }
    }
}
