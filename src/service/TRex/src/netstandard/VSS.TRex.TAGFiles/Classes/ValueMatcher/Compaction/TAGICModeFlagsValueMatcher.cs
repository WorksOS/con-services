using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction
{
    public class TAGICModeFlagsValueMatcher : TAGValueMatcher
    {
        public TAGICModeFlagsValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICModeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.ICMode = (byte)value;
                result = true;
            }

            return result;
        }
    }
}
