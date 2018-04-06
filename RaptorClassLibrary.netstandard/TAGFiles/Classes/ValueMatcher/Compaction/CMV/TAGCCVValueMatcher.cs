using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CMV
{
    /// <summary>
    /// Handles absolute and offset Compaction Meter Values
    /// </summary>
    public class TAGCCVValueMatcher : TAGValueMatcher
    {
        public TAGCCVValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCVTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteCCV = false;

            valueSink.SetICCCVValue(CellPass.NullCCV);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteCCV)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((short)(valueSink.ICCCVValues.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICCCVValue((short)((short)(valueSink.ICCCVValues.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteCCV = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICCCVValue((short)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
