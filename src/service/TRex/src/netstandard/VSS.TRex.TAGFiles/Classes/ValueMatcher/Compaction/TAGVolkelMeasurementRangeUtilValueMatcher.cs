using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction
{
    /// <summary>
    /// Handles the Volkel sensor util range measurement recorded byt he machine
    /// </summary>
    public class TAGVolkelMeasurementRangeUtilValueMatcher : TAGValueMatcher
    {
        public TAGVolkelMeasurementRangeUtilValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTabFileVolkelMeasRangeUtil };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteVolkelMeasUtilRange = false;

            valueSink.SetVolkelMeasUtilRange(CellPassConsts.NullVolkelMeasUtilRange);

            return true;
        }

        public override bool ProcessIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, int value)
        {
            bool result = false;

            if (state.HaveSeenAnAbsoluteVolkelMeasUtilRange &&
                (valueType.Type == TAGDataType.t4bitInt || valueType.Type == TAGDataType.t8bitInt))
            { 
                if (((int)valueSink.VolkelMeasureUtilRanges.GetLatest() + value) >= 0)
                {    
                    valueSink.SetVolkelMeasUtilRange((int)valueSink.VolkelMeasureUtilRanges.GetLatest() + value);
                    result = true;
                }
            }

            return result;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t12bitUInt)
            {
              valueSink.SetVolkelMeasUtilRange((int) value);
              state.HaveSeenAnAbsoluteVolkelMeasUtilRange = true;
              result = true;
            }

            return result;
        }
    }
}
