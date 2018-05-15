using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGOnGroundValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileOnGroundTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t4bitUInt)
            {
                return false;
            }

            switch (value)
            {
                case 0:
                    valueSink.SetOnGround(OnGroundState.No);
                    break;
                case 1:
                    valueSink.SetOnGround(OnGroundState.YesLegacy);
                    break;
                default:
                    valueSink.SetOnGround(OnGroundState.Unknown);
                    break;
            }

            return true;
        }
    }
}
