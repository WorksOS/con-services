using VSS.TRex.Common.Types;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    ///  Handles the machine direction TAG value
    /// </summary>
    public class TAGDirectionValueMatcher : TAGValueMatcher
    {
        public TAGDirectionValueMatcher()
        {
        }

        private static readonly string[] valueTypes = { TAGValueNames.kTagFileDirectionTag };

        public override string[] MatchedValueTypes() => valueTypes;

        public override bool ProcessUnsignedIntegerValue(TAGValueMatcherState state, TAGProcessorStateBase valueSink,
          TAGDictionaryItem valueType, uint value)
        {
            bool result = false;

            var testValue = value - 1; // Direction value in tag file is 1-based
            if (valueType.Type == TAGDataType.t4bitUInt &&  
                (testValue >= MachineDirectionConsts.MACHINE_DIRECTION_MIN_VALUE && testValue <= MachineDirectionConsts.MACHINE_DIRECTION_MAX_VALUE)) 
            {
                 valueSink.MachineDirection = (MachineDirection) testValue;
                 result = true;
            }

            return result;
        }
    }
}
