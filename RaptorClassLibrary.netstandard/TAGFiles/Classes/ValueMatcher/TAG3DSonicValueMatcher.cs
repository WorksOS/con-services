using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    public class TAG3DSonicValueMatcher : TAGValueMatcher
    {
        public TAG3DSonicValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTag3DSonic };
        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!(valueType.Type == TAGDataType.t4bitUInt && value <= 2)) // Sonic state currently only defines three states
            {
                return false;
            }

            valueSink.ICSonic3D = (byte)value;
            return true;
        }
    }
}
