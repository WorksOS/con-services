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

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileVolkelMeasRange };

        public override string[] MatchedValueTypes() => valueTypes;

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
