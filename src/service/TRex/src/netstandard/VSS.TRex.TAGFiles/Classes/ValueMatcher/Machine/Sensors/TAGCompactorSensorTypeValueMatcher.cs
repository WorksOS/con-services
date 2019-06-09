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
            if (!(value >= CompactionSensorTypeConsts.COMPACTION_SENSOR_TYPE_MIN_VALUE && value <= CompactionSensorTypeConsts.COMPACTION_SENSOR_TYPE_MAX_VALUE))
            {
                return false;
            }

            valueSink.ICSensorType = (CompactionSensorType)value;

            return true;
        }
    }
}
