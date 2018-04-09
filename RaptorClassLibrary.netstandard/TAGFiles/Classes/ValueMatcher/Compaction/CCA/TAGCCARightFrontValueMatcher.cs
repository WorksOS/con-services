using VSS.VisionLink.Raptor.Cells;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.ValueMatcher.Compaction.CCA
{
    public class TAGCCARightFrontValueMatcher : TAGValueMatcher
  {
    public TAGCCARightFrontValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

      private static readonly string[] valueTypes = { TAGValueNames.kTagFileICCCARightFrontTag };

      public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessEmptyValue(TAGDictionaryItem valueType)
    {
      valueSink.SetICCCARightFrontValue(CellPass.NullCCA);

      return true;
    }

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      if (valueType.Type != TAGDataType.t8bitUInt)
      {
        return false;
      }

      valueSink.SetICCCARightFrontValue((byte)value);

      return true;
    }
  }
}
