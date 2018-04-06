using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CMV
{
    public class TAGMDPValueMatcher : TAGValueMatcher
    {
        public TAGMDPValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICMDPTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            state.HaveSeenAnAbsoluteMDP = false;

            valueSink.SetICMDPValue(CellPass.NullMDP);

            return true;
        }

        public override bool ProcessIntegerValue(TAGDictionaryItem valueType, int value)
        {
            if (!state.HaveSeenAnAbsoluteMDP)
            {
                return false;
            }

            switch (valueType.Type)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t8bitInt:
                    if (((short)(valueSink.ICMDPValues.GetLatest()) + value) < 0)
                    {
                        return false;
                    }

                    valueSink.SetICMDPValue((short)((short)(valueSink.ICMDPValues.GetLatest()) + value));
                    break;

                default:
                    return false;
            }

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            state.HaveSeenAnAbsoluteMDP = true;

            switch (valueType.Type)
            {
                case TAGDataType.t12bitUInt:
                    valueSink.SetICMDPValue((short)value);
                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}
