using VSS.TRex.Cells;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCAValueMatcher : TAGValueMatcher
    {
        public TAGCCAValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCATag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
        {
            valueSink.SetICCCAValue(CellPass.NullCCA);

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
        {
            if (valueType.Type != TAGDataType.t8bitUInt)
            {
                return false;
            }

            valueSink.SetICCCAValue((byte)value);

            return true;
        }
    }
}
