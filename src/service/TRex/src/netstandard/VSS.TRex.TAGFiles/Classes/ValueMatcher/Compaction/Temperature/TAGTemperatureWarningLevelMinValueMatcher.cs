﻿using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Compaction.Temperature
{
  public class TAGTemperatureWarningLevelMinValueMatcher : TAGValueMatcher
  {
    public TAGTemperatureWarningLevelMinValueMatcher(TAGProcessorStateBase valueSink, TAGValueMatcherState state) : base(valueSink, state)
    {
    }

    private static readonly string[] valueTypes = {TAGValueNames.kTempLevelMinTag};

    public override string[] MatchedValueTypes() => valueTypes;

    public override bool ProcessUnsignedIntegerValue(TAGDictionaryItem valueType, uint value)
    {
      bool result = false;

      // Value is a minimum temperature warning level value...
      if (valueType.Type == TAGDataType.t12bitUInt)
      {
        valueSink.ICTempWarningLevelMinValue = (ushort) (value * CellPassConsts.MaterialTempValueRatio);
        result = true;
      }

      return result;
    }
  }
}
