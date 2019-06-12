using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCARightFrontValueMatcher : TAGValueMatcher
  {
    public TAGCCARightFrontValueMatcher()
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCARightFrontTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType)
    {
      valueSink.SetICCCARightFrontValue(CellPassConsts.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Type == TAGDataType.t8bitUInt)
      {
          valueSink.SetICCCARightFrontValue((byte) value);
          result = true;
      }

      return result;
    }
  }
}
