using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCALeftFrontValueMatcher : TAGValueMatcher
  {
    public TAGCCALeftFrontValueMatcher()
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCALeftFrontTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType)
    {
      valueSink.SetICCCALeftFrontValue(CellPassConsts.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Type == TAGDataType.t8bitUInt)
      {
          valueSink.SetICCCALeftFrontValue((byte) value);
          result = true;
      }

      return result;
    }
  }
}
