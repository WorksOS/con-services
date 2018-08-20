using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Vibratory
{
    /// <summary>
    /// Handles vibratory drum vibration frequency Values reported by the machine 
    /// </summary>
    public class TAGFrequencyValueMatcher : TAGValueMatcher
    {
        public TAGFrequencyValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICFrequencyTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteFrequency = false;

            valueSink.SetICFrequency(CellPassConsts.NullFrequency);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteFrequency)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((ushort)(valueSink.ICFrequencys.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICFrequency((ushort)((ushort)(valueSink.ICFrequencys.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteFrequency = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICFrequency((ushort)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
