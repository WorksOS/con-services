using VSS.TRex.Cells;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCARightRearValueMatcher : TAGValueMatcher
  {
    public TAGCCARightRearValueMatcher()
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCARightRearTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType)
    {
      valueSink.SetICCCARightRearValue(CellPassConsts.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
      TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      if (valueType.Type == TAGDataType.t8bitUInt)
      {
          valueSink.SetICCCARightRearValue((byte)value);
          result = true;
      }

      return result;
    }
  }
}
