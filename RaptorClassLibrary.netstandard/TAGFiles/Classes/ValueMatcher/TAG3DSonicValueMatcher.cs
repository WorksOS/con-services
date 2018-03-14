using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher
{
    public class TAG3DSonicValueMatcher : TAGValueMatcher
    {
        public TAG3DSonicValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTag3DSonic };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!(valueType.Type == TAGDataType.t4bitUInt && (value >= 0) && value <= 2)) // Sonic state currently only defines three states
            {
                return false;
            }

            valueSink.ICSonic3D = (byte)value;
            return true;
        }
    }
}
