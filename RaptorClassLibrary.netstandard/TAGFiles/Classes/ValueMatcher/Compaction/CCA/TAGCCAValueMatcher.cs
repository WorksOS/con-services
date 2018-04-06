using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA
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

            valueSink.SetICCCAValue((short)value);

            return true;
        }
    }
}
