using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Sensors
{
    public class TAGCompactorSensorTypeValueMatcher : TAGValueMatcher
    {
        public TAGCompactorSensorTypeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileCompactorSensorType };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            if (!Enum.IsDefined(typeof(CompactionSensorType), (byte)value))
            {
                return false;
            }

            CompactionSensorType sensorType = (CompactionSensorType)value;
            valueSink.ICSensorType = sensorType;

            return true;
        }
    }
}
