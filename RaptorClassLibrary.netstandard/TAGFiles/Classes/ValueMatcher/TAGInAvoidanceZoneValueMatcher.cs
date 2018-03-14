using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// Handle the avoidance zone entry/exit state reported by the machine
    /// </summary>
    public class TAGInAvoidanceZoneValueMatcher : TAGValueMatcher
    {
        public TAGInAvoidanceZoneValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagInAvoidZone };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!(valueType.Type == TAGDataType.t4bitUInt && (value >= 0) && value <= 3)) // Check only the two least significant bits are set
            {
                return false;
            }

            valueSink.SetInAvoidZoneState((byte)value);

            return true;
        }
    }
}
