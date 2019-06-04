using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGOnGroundValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileOnGroundTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt)
            {
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

              result = true;
            }

            return result;
        }
    }
}
