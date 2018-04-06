using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction
{
    public class TAGICModeFlagsValueMatcher : TAGValueMatcher
    {
        public TAGICModeFlagsValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICModeTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.ICMode = (byte)value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
