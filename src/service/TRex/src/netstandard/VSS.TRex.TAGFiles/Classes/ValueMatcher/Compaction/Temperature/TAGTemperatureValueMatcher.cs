using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
    /// <summary>
    ///  Handles temperature measurements supplied by asphalt compactors
    /// </summary>
    public class TAGTemperatureValueMatcher : TAGValueMatcher
    {
        public TAGTemperatureValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTemperatureTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteTemperature = false;

            valueSink.SetICTemperatureValue(CellPassConsts.NullMaterialTemperatureValue);

            return true;
        }

        public override bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (state.HaveSeenAnAbsoluteTemperature)
            {
              if (valueType.Type == TAGDataType.t4bitInt || valueType.Type == TAGDataType.t8bitInt)
              {
                if ((ushort) valueSink.ICTemperatureValues.GetLatest() + value >= 0)
                {
                  valueSink.SetICTemperatureValue((ushort) ((ushort) valueSink.ICTemperatureValues.GetLatest() + value));
                  result = true;
                }
              }
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
          // Value is absolute temperature value
          state.HaveSeenAnAbsoluteTemperature = true;

          bool result = false;

          if (valueType.Type == TAGDataType.t12bitUInt)
          {
            valueSink.SetICTemperatureValue((ushort) value);
            result = true;
          }

          return result;
        }
    }
}
