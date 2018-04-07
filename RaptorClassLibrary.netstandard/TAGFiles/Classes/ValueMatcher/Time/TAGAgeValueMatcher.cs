using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Time
{
    public class TAGAgeValueMatcher : TAGValueMatcher
    {
        public TAGAgeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileCorrectionAgeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.SetAgeOfCorrection((byte) value);
                return true;
            }

            return false;
        }
    }
}
