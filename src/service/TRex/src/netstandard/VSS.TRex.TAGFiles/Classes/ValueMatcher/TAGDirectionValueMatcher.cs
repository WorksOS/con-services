using System;
using VSS.TRex.Common.Types;
using VSS.TRex.TAGFiles.Classes.States;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

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

            if (valueType.Type == TAGDataType.t4bitUInt &&             
                Enum.IsDefined(typeof(MachineDirection), (byte) (value - 1))) // Direction value in tag file is 1-based
            {
                 valueSink.MachineDirection = (MachineDirection) (value - 1);
                 result = true;
            }

            return result;
        }
    }
}
