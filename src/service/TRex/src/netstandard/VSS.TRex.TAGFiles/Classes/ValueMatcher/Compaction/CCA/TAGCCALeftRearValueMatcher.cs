using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCALeftRearValueMatcher : TAGValueMatcher
  {
    public TAGCCALeftRearValueMatcher()
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCALeftRearTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType)
    {
      valueSink.SetICCCALeftRearValue(CellPassConsts.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Type == TAGDataType.t8bitUInt)
      {
          valueSink.SetICCCALeftRearValue((byte) value);
          result = true;
      }

      return result;
    }
  }
}
