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
            if (!state.HaveSeenAnAbsoluteVolkelMeasUtilRange)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((int)valueSink.VolkelMeasureUtilRanges.GetLatest() + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetVolkelMeasUtilRange((int)valueSink.VolkelMeasureUtilRanges.GetLatest() + value);
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t12bitUInt)
            {
                return false;
            }

            valueSink.SetVolkelMeasUtilRange((int)value);
            state.HaveSeenAnAbsoluteVolkelMeasUtilRange = true;

            return true;
        }
    }
}
