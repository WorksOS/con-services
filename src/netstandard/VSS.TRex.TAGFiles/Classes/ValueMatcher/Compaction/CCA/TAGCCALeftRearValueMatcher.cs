using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCALeftRearValueMatcher : TAGValueMatcher
  {
    public TAGCCALeftRearValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCALeftRearTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
    {
      valueSink.SetICCCALeftRearValue(CellPassConsts.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      if (valueType.Type != TAGDataType.t8bitUInt)
      {
        return false;
      }

      valueSink.SetICCCALeftRearValue((byte)value);

      return true;
    }
  }
}
