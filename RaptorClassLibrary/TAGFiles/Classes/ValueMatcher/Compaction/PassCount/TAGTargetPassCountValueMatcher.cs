using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.PassCount
{
    /// <summary>
    /// Handles Target CCV vlaues set on the machine by an operator
    /// </summary>
    public class TAGTargetPassCountValueMatcher : TAGValueMatcher
    {
        public TAGTargetPassCountValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileICPassTargetTag };
        }

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is the absolute CCV value
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
                valueSink.ICPassTargetValue = (ushort)value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
