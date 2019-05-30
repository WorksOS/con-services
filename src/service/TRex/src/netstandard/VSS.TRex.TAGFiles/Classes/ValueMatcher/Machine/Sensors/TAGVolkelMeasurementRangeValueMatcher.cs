using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine.Sensors
{
    /// <summary>
    /// Handles the Volker compaction sensor measurment range from the machine
    /// </summary>
    public class TAGVolkelMeasurementRangeValueMatcher : TAGValueMatcher
    {
        public TAGVolkelMeasurementRangeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileVolkelMeasRange };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt)
            {
                valueSink.SetVolkelMeasRange((byte)value);
                result = true;
            }

            return result;
        }
    }
}
