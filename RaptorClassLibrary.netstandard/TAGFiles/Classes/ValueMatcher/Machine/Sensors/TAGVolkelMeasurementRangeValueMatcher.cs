using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Sensors
{
    /// <summary>
    /// Handles the Volker compaction sensor measurment range from the machine
    /// </summary>
    public class TAGVolkelMeasurementRangeValueMatcher : TAGValueMatcher
    {
        public TAGVolkelMeasurementRangeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileVolkelMeasRange };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.SetVolkelMeasRange((int)value);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
