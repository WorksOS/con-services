using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
    public class TAGTemperatureWarningLevelMinValueMatcher : TAGValueMatcher
    {
        public TAGTemperatureWarningLevelMinValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTempLevelMinTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is a minimum temperature warning level value...

            if (valueType.Type != TAGDataType.t12bitUInt)
            {
                return false;
            }

            valueSink.ICTempWarningLevelMinValue = (ushort)(value * CellPass.MaterialTempValueRatio);

            return true;
        }
    }
}
