﻿using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGMachineTypeValueMatcher : TAGValueMatcher
    {
        public TAGMachineTypeValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagMachineType };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t8bitUInt) 
            {
              valueSink.MachineType = (MachineType)value;
              result = true;
            }

            return result;
        }
    }
}
