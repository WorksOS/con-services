using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGOnGroundValueMatcher : TAGValueMatcher
    {
        public TAGOnGroundValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileOnGroundTag };
        }

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
