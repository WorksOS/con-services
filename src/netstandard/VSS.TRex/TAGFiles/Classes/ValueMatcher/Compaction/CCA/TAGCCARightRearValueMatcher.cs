using VSS.TRex.Cells;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCARightRearValueMatcher : TAGValueMatcher
  {
    public TAGCCARightRearValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCARightRearTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
    {
      valueSink.SetICCCARightRearValue(CellPass.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      if (valueType.Type != TAGDataType.t8bitUInt)
      {
        return false;
      }

      valueSink.SetICCCARightRearValue((byte)value);

      return true;
    }
  }
}
