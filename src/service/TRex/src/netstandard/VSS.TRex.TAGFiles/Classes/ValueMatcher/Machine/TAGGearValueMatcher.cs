using System;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher.Machine
{
    public class TAGGearValueMatcher : TAGValueMatcher
    {
        public TAGGearValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileICGearTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            if (valueType.Type == TAGDataType.t4bitUInt &&
                Enum.IsDefined(typeof(MachineGear), (byte)value))
            {
              valueSink.ICGear = (MachineGear)value;
              result = true;
            }

            return result;
        }
    }
}
