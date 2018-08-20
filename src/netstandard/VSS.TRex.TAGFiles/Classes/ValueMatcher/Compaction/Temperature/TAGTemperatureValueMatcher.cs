using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
    /// <summary>
    ///  Handles temperature measurements supplied by asphalt compactors
    /// </summary>
    public class TAGTemperatureValueMatcher : TAGValueMatcher
    {
        public TAGTemperatureValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTemperatureTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteTemperature = false;

            valueSink.SetICTemperatureValue(CellPassConsts.NullMaterialTemperatureValue);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteTemperature)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if ((ushort)valueSink.ICTemperatureValues.GetLatest() + value < 0)
                    {
                        return false;
                    }

                    valueSink.SetICTemperatureValue((ushort)((ushort)valueSink.ICTemperatureValues.GetLatest() + value));

                    break;
                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            // Value is abosulte temperature value
            state.HaveSeenAnAbsoluteTemperature = true;

            if (valueType.Type != TAGDataType.t12bitUInt)
            {
                return false;
            }

            valueSink.SetICTemperatureValue((ushort)value);

            return true;
        }
    }
}
