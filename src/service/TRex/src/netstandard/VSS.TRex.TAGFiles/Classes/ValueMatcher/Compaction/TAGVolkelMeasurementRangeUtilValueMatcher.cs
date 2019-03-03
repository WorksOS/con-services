using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction
{
    /// <summary>
    /// Handles the Volkel sensor util range measurement recorded byt he machine
    /// </summary>
    public class TAGVolkelMeasurementRangeUtilValueMatcher : TAGValueMatcher
    {
        public TAGVolkelMeasurementRangeUtilValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTabFileVolkelMeasRangeUtil };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteVolkelMeasUtilRange = false;

            valueSink.SetVolkelMeasUtilRange(CellPassConsts.NullVolkelMeasUtilRange);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
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

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
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
