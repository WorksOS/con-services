using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Time
{
    public class TAGAgeValueMatcher : TAGValueMatcher
    {
        public TAGAgeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileCorrectionAgeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.SetAgeOfCorrection((byte) value);
                result = true;
            }

            return result;
        }
    }
}
