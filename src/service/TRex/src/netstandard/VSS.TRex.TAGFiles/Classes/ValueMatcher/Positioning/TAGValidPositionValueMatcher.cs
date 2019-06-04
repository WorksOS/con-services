using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Positioning
{
    public class TAGValidPositionValueMatcher : TAGValueMatcher
    {
        public TAGValidPositionValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileValidPositionTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.ValidPosition = (byte)value;
                result = true;
            }

            return result;
        }
    }
}
