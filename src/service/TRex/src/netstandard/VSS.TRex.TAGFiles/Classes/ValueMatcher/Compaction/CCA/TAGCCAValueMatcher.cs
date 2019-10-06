using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCAValueMatcher : TAGValueMatcher
    {
        public TAGCCAValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCATag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType)
        {
            valueSink.SetICCCAValue(CellPassConsts.NullCCA);

            return true;
        }

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;
            
            if (valueType.Type == TAGDataType.t8bitUInt)
            {
                valueSink.SetICCCAValue((byte)value);
                result = true;
            }

            return result;
        }
    }
}
