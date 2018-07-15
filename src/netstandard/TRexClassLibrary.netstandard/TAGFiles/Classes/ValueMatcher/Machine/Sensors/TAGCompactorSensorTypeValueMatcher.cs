using System;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Sensors
{
    public class TAGCompactorSensorTypeValueMatcher : TAGValueMatcher
    {
        public TAGCompactorSensorTypeValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileCompactorSensorType };

        public override string[] MatchedValueTypes() => valueTypes;

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
