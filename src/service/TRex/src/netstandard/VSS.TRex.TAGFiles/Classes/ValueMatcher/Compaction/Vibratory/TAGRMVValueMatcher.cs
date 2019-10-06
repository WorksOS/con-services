using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles Resonance Meter Values reported by the machine mounted compaction sensor
    /// </summary>
    public class TAGRMVValueMatcher : TAGValueMatcher
    {
        public TAGRMVValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICRMVTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteRMV = false;

            valueSink.SetICRMVValue(CellPassConsts.NullRMV);

            return true;
        }

        public override bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (state.HaveSeenAnAbsoluteRMV && 
                (valueType.Type == TAGDataType.t4bitInt || valueType.Type == TAGDataType.t8bitInt))
            {
                if (((short) (valueSink.ICRMVValues.GetLatest()) + value) >= 0)
                {
                     valueSink.SetICRMVValue((short) ((short) (valueSink.ICRMVValues.GetLatest()) + value));
                     result = true;
                }
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteRMV = true;

            bool result = false;

            if (valueType.Type == TAGDataType.t12bitUInt)
            { 
                valueSink.SetICRMVValue((short)value);
                result = true;
            }

            return result;
        }
    }
}
