using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
    public class TAGTemperatureWarningLevelMaxValueMatcher : TAGValueMatcher
    {
        public TAGTemperatureWarningLevelMaxValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTempLevelMaxTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            // Value is a minimum temperature warning level value...
            if (valueType.Type == TAGDataType.t12bitUInt)
            {
              valueSink.ICTempWarningLevelMaxValue = (ushort) (value * CellPassConsts.MaterialTempValueRatio);
              result = true;
            }

            return result;
        }
    }
}
