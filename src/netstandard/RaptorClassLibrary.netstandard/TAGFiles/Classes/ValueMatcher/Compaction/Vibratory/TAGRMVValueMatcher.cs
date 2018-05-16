using VSS.TRex.Cells;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles Resonance Meter Values reported by the machine mounted compaction sensor
    /// </summary>
    public class TAGRMVValueMatcher : TAGValueMatcher
    {
        public TAGRMVValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICRMVTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteRMV = false;

            valueSink.SetICRMVValue(CellPass.NullRMV);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteRMV)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((short)(valueSink.ICRMVValues.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICRMVValue((short)((short)(valueSink.ICRMVValues.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteRMV = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICRMVValue((short)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
