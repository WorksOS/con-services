using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Machine.Sensors
{
    public class TAGCompactorSensorTypeValueMatcher : TAGValueMatcher
    {
        public TAGCompactorSensorTypeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        public override string[] MatchedValueTypes()
        {
            return new string[] { TAGValueNames.kTagFileCompactorSensorType };
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (!Enum.IsDefined(typeof(CompactionSensorType), (int)value))
            {
                return false;
            }

            CompactionSensorType sensorType = (CompactionSensorType)value;
            valueSink.ICSensorType = sensorType;

            return true;
        }
    }
}
